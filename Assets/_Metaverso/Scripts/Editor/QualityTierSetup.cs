using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Metaverso.EditorTools
{
    /// <summary>
    /// Renderers Web_Quest y Web_PC, un asset URP por nivel y los niveles Web_Quest / Web_PC / Web_Ultra
    /// de QualitySettings, todo desde QualityTierRules.Profile. Corre en cada build web: lo que se cambie
    /// a mano en esos assets se pisa. El look (tonemapping, bloom) vive en Web_PC_Volume y no se pisa.
    /// </summary>
    public static class QualityTierSetup
    {
        public const string SettingsFolder = "Assets/Settings";
        public const string VolumeProfilePath = SettingsFolder + "/Web_PC_Volume.asset";

        const string LegacyWebAsset = SettingsFolder + "/Web_RPAsset.asset";
        const string QuestRendererTemplate = SettingsFolder + "/Mobile_Renderer.asset";
        const string PcRendererTemplate = SettingsFolder + "/PC_Renderer.asset";
        const string PipelineTemplate = SettingsFolder + "/PC_RPAsset.asset";
        const string QualitySettingsPath = "ProjectSettings/QualitySettings.asset";
        const string WebGpuPref = "Metaverso.WebGpuSpike";
        const string WebGpuMenu = "Metaverso/Rendering/WebGPU en desktop (spike)";

        static readonly string[] ExcludedPlatforms = { "Standalone", "Android", "iPhone" };
        static readonly QualityTier[] Tiers = { QualityTier.Quest, QualityTier.Pc, QualityTier.Ultra };

        /// <summary>Preferencia de esta maquina. Apagado: desktop y Quest salen solo con WebGL2.</summary>
        public static bool WebGpuSpikeEnabled => EditorPrefs.GetBool(WebGpuPref, false);

        [MenuItem("Metaverso/Rendering/Configurar niveles de calidad")]
        public static void ApplyFromMenu()
        {
            if (Apply())
                Debug.Log("[Metaverso] Niveles Web_Quest, Web_PC y Web_Ultra listos en " + SettingsFolder + ".");
        }

        [MenuItem(WebGpuMenu)]
        static void ToggleWebGpu()
        {
            EditorPrefs.SetBool(WebGpuPref, !WebGpuSpikeEnabled);
            Debug.Log(WebGpuSpikeEnabled
                ? "[Metaverso] WebGPU encendido para el build desktop (WebGPU primero, WebGL2 de respaldo). Quest sigue en WebGL2."
                : "[Metaverso] WebGPU apagado: desktop y Quest salen con WebGL2.");
        }

        [MenuItem(WebGpuMenu, true)]
        static bool ToggleWebGpuValidate()
        {
            Menu.SetChecked(WebGpuMenu, WebGpuSpikeEnabled);
            return true;
        }

        public static bool Apply()
        {
            var questRenderer = EnsureRenderer(QualityTierRules.Profile(QualityTier.Quest), QuestRendererTemplate);
            var pcRenderer = EnsureRenderer(QualityTierRules.Profile(QualityTier.Pc), PcRendererTemplate);
            if (questRenderer == null || pcRenderer == null)
                return false;

            var volume = EnsureVolumeProfile();
            var pipelines = new Dictionary<QualityTier, UniversalRenderPipelineAsset>();
            foreach (var tier in Tiers)
            {
                var profile = QualityTierRules.Profile(tier);
                var renderer = profile.Renderer == questRenderer.name ? questRenderer : pcRenderer;
                var pipeline = EnsurePipeline(profile, renderer, volume);
                if (pipeline == null)
                    return false;
                pipelines[tier] = pipeline;
            }

            if (!EnsureQualityLevels(pipelines))
                return false;

            AssetDatabase.SaveAssets();
            if (File.Exists(LegacyWebAsset))
                AssetDatabase.DeleteAsset(LegacyWebAsset);
            return true;
        }

        /// <summary>Lista de APIs y nivel por defecto de la variante. Devuelve lo que queda en el build.</summary>
        public static string PrepareVariant(bool quest)
        {
            var webGpu = !quest && WebGpuSpikeEnabled;
            var apis = webGpu
                ? new[] { GraphicsDeviceType.WebGPU, GraphicsDeviceType.OpenGLES3 }
                : new[] { GraphicsDeviceType.OpenGLES3 };
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            var current = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
            if (!SameApis(current, apis))
                PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, apis);

            SetWebDefaultLevel(QualityTierRules.Profile(quest ? QualityTier.Quest : QualityTier.Pc).QualityLevel);
            AssetDatabase.SaveAssets();
            return GraphicsLabel(webGpu);
        }

        public static string GraphicsLabel(bool webGpu) => webGpu ? "webgpu+webgl2" : "webgl2";

        static UniversalRendererData EnsureRenderer(QualityTierProfile profile, string templatePath)
        {
            var path = SettingsFolder + "/" + profile.Renderer + ".asset";
            if (!File.Exists(path) && !CopyTemplate(templatePath, path))
                return null;

            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (data == null)
            {
                Debug.LogError("[Metaverso] " + path + " no es un UniversalRendererData.");
                return null;
            }

            var so = new SerializedObject(data);
            so.FindProperty("m_RenderingMode").intValue = (int)RenderingMode.Forward;
            so.ApplyModifiedPropertiesWithoutUndo();

            var hasSsao = false;
            foreach (var feature in data.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion)
                {
                    feature.SetActive(profile.Ssao);
                    hasSsao = true;
                }
            }

            if (profile.Ssao && !hasSsao)
                Debug.LogWarning("[Metaverso] " + path + " no tiene SSAO. Agregalo como Renderer Feature.");
            EditorUtility.SetDirty(data);
            return data;
        }

        static UniversalRenderPipelineAsset EnsurePipeline(QualityTierProfile profile, UniversalRendererData renderer, VolumeProfile volume)
        {
            var path = SettingsFolder + "/" + profile.PipelineAsset + ".asset";
            if (!File.Exists(path) && !CopyTemplate(PipelineTemplate, path))
                return null;

            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset == null)
            {
                Debug.LogError("[Metaverso] " + path + " no es un UniversalRenderPipelineAsset.");
                return null;
            }

            var so = new SerializedObject(asset);
            var renderers = so.FindProperty("m_RendererDataList");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            so.FindProperty("m_DefaultRendererIndex").intValue = 0;
            so.FindProperty("m_SupportsHDR").boolValue = profile.Hdr;
            so.FindProperty("m_MSAA").intValue = profile.Msaa;
            so.FindProperty("m_RenderScale").floatValue = profile.RenderScale;
            so.FindProperty("m_RequireDepthTexture").boolValue = false;
            so.FindProperty("m_RequireOpaqueTexture").boolValue = false;
            so.FindProperty("m_MainLightShadowsSupported").boolValue = profile.MainLightShadows;
            so.FindProperty("m_MainLightShadowmapResolution").intValue = profile.MainShadowResolution;
            so.FindProperty("m_ShadowCascadeCount").intValue = profile.ShadowCascades;
            so.FindProperty("m_ShadowDistance").floatValue = profile.ShadowDistance;
            so.FindProperty("m_SoftShadowsSupported").boolValue = profile.SoftShadows;
            so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = profile.AdditionalLightsPerObject;
            so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = profile.AdditionalLightShadows;
            so.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = Mathf.Max(512, profile.MainShadowResolution / 2);
            so.FindProperty("m_ReflectionProbeBlending").boolValue = profile.ReflectionProbeBlending;
            so.FindProperty("m_ReflectionProbeBoxProjection").boolValue = true;
            so.FindProperty("m_ColorGradingMode").intValue = (int)(profile.Hdr ? ColorGradingMode.HighDynamicRange : ColorGradingMode.LowDynamicRange);
            so.FindProperty("m_UseFastSRGBLinearConversion").boolValue = !profile.Hdr;
            so.FindProperty("m_VolumeProfile").objectReferenceValue = profile.PostProcessing ? volume : null;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static VolumeProfile EnsureVolumeProfile()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (existing != null)
                return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            var tonemapping = profile.Add<Tonemapping>();
            tonemapping.mode.Override(TonemappingMode.ACES);
            var bloom = profile.Add<Bloom>();
            bloom.threshold.Override(1f);
            bloom.intensity.Override(0.15f);
            bloom.scatter.Override(0.65f);
            foreach (var component in profile.components)
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            EditorUtility.SetDirty(profile);
            return profile;
        }

        static bool EnsureQualityLevels(Dictionary<QualityTier, UniversalRenderPipelineAsset> pipelines)
        {
            var so = QualitySettingsObject();
            if (so == null)
                return false;

            var levels = so.FindProperty("m_QualitySettings");
            var pcName = QualityTierRules.Profile(QualityTier.Pc).QualityLevel;
            if (IndexOf(levels, pcName) < 0)
            {
                var legacy = IndexOf(levels, "Web");
                if (legacy >= 0)
                {
                    levels.GetArrayElementAtIndex(legacy).FindPropertyRelative("name").stringValue = pcName;
                }
                else
                {
                    levels.InsertArrayElementAtIndex(levels.arraySize - 1);
                    levels.GetArrayElementAtIndex(levels.arraySize - 1).FindPropertyRelative("name").stringValue = pcName;
                }
            }

            var questName = QualityTierRules.Profile(QualityTier.Quest).QualityLevel;
            if (IndexOf(levels, questName) < 0)
            {
                var pc = IndexOf(levels, pcName);
                levels.InsertArrayElementAtIndex(pc);
                levels.GetArrayElementAtIndex(pc).FindPropertyRelative("name").stringValue = questName;
            }

            var ultraName = QualityTierRules.Profile(QualityTier.Ultra).QualityLevel;
            if (IndexOf(levels, ultraName) < 0)
            {
                var pc = IndexOf(levels, pcName);
                levels.InsertArrayElementAtIndex(pc);
                levels.GetArrayElementAtIndex(pc + 1).FindPropertyRelative("name").stringValue = ultraName;
            }

            foreach (var tier in Tiers)
            {
                var profile = QualityTierRules.Profile(tier);
                var level = levels.GetArrayElementAtIndex(IndexOf(levels, profile.QualityLevel));
                level.FindPropertyRelative("customRenderPipeline").objectReferenceValue = pipelines[tier];
                level.FindPropertyRelative("lodBias").floatValue = profile.LodBias;
                level.FindPropertyRelative("globalTextureMipmapLimit").intValue = profile.TextureMipLimit;
                level.FindPropertyRelative("antiAliasing").intValue = 0;
                level.FindPropertyRelative("anisotropicTextures").intValue = tier == QualityTier.Quest ? (int)AnisotropicFiltering.Enable : (int)AnisotropicFiltering.ForceEnable;
                level.FindPropertyRelative("realtimeReflectionProbes").boolValue = false;
                level.FindPropertyRelative("maximumLODLevel").intValue = 0;
                var excluded = level.FindPropertyRelative("excludedTargetPlatforms");
                excluded.arraySize = ExcludedPlatforms.Length;
                for (var i = 0; i < ExcludedPlatforms.Length; i++)
                    excluded.GetArrayElementAtIndex(i).stringValue = ExcludedPlatforms[i];
            }

            SetWebDefault(so, IndexOf(levels, pcName));
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        static void SetWebDefaultLevel(string levelName)
        {
            var so = QualitySettingsObject();
            if (so == null)
                return;
            var index = IndexOf(so.FindProperty("m_QualitySettings"), levelName);
            if (index < 0)
            {
                Debug.LogWarning("[Metaverso] Falta el nivel " + levelName + ". Usa Metaverso > Rendering > Configurar niveles de calidad.");
                return;
            }

            SetWebDefault(so, index);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetWebDefault(SerializedObject so, int index)
        {
            var defaults = so.FindProperty("m_PerPlatformDefaultQuality");
            for (var i = 0; i < defaults.arraySize; i++)
            {
                var entry = defaults.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue != "WebGL")
                    continue;
                entry.FindPropertyRelative("second").intValue = index;
                return;
            }

            defaults.InsertArrayElementAtIndex(defaults.arraySize);
            var added = defaults.GetArrayElementAtIndex(defaults.arraySize - 1);
            added.FindPropertyRelative("first").stringValue = "WebGL";
            added.FindPropertyRelative("second").intValue = index;
        }

        static SerializedObject QualitySettingsObject()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(QualitySettingsPath);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[Metaverso] No pude abrir " + QualitySettingsPath + ".");
                return null;
            }

            return new SerializedObject(assets[0]);
        }

        static int IndexOf(SerializedProperty levels, string name)
        {
            for (var i = 0; i < levels.arraySize; i++)
            {
                if (levels.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                    return i;
            }

            return -1;
        }

        static bool CopyTemplate(string templatePath, string path)
        {
            if (!File.Exists(templatePath))
            {
                Debug.LogError("[Metaverso] Falta la plantilla " + templatePath + " para crear " + path + ".");
                return false;
            }

            if (!AssetDatabase.CopyAsset(templatePath, path))
            {
                Debug.LogError("[Metaverso] No pude copiar " + templatePath + " a " + path + ".");
                return false;
            }

            return true;
        }

        static bool SameApis(GraphicsDeviceType[] current, GraphicsDeviceType[] wanted)
        {
            if (current == null || current.Length != wanted.Length)
                return false;
            for (var i = 0; i < wanted.Length; i++)
            {
                if (current[i] != wanted[i])
                    return false;
            }

            return true;
        }
    }
}
