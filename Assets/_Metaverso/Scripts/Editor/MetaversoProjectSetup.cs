using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metaverso.EditorTools
{
    [InitializeOnLoad]
    public static class MetaversoProjectSetup
    {
        const string PrefKey = "Metaverso.CoreSetupDone";

        static MetaversoProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(PrefKey, false))
                    return;
                ApplyPlayerSettings();
                if (!File.Exists(WebBuildPipeline.ScenePath))
                    TestWorldBuilder.CreateOrReplace();
                EditorPrefs.SetBool(PrefKey, true);
            };
        }

        [MenuItem("Metaverso/Configurar proyecto Web")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Sinestesia";
            PlayerSettings.productName = "MetaversoSinestesia";
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Master);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = 64;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            if (InputSystem.settings != null)
            {
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                EditorUtility.SetDirty(InputSystem.settings);
            }

            Debug.Log("[Metaverso] Player Settings web aplicados. Color space lineal se mantiene para la arquitectura.");
        }

        [MenuItem("Metaverso/Red/Activar Photon Fusion")]
        public static void ActivatePhoton()
        {
            var hasFusion = false;
            foreach (var assembly in UnityEditor.Compilation.CompilationPipeline.GetAssemblies())
            {
                if (assembly.name == "Fusion.Runtime")
                {
                    hasFusion = true;
                    break;
                }
            }

            if (!hasFusion)
            {
                EditorUtility.DisplayDialog(
                    "Photon Fusion",
                    "Fusion todavia no esta en el proyecto.\n\n1. Crea una app en dashboard.photonengine.com (Fusion).\n2. Descarga el SDK de Fusion 2 e importalo.\n3. Vuelve a este menu.\n\nEl App ID se pega en PhotonAppSettings, el asset que crea el SDK.",
                    "Ok");
                return;
            }

            var group = NamedBuildTarget.WebGL;
            var defines = PlayerSettings.GetScriptingDefineSymbols(group);
            if (!defines.Contains("PHOTON_FUSION"))
            {
                defines = string.IsNullOrEmpty(defines) ? "PHOTON_FUSION" : defines + ";PHOTON_FUSION";
                PlayerSettings.SetScriptingDefineSymbols(group, defines);
            }

            var asmdefPath = "Assets/_Metaverso/Scripts/Network/Fusion/Metaverso.Fusion.asmdef";
            var json = File.ReadAllText(asmdefPath);
            if (!json.Contains("Fusion.Runtime"))
            {
                json = json.Replace("\"Metaverso.Runtime\"", "\"Metaverso.Runtime\",\n    \"Fusion.Runtime\"");
                File.WriteAllText(asmdefPath, json);
                AssetDatabase.ImportAsset(asmdefPath);
            }

            EditorUtility.DisplayDialog(
                "Photon Fusion",
                "Define PHOTON_FUSION activo y Fusion.Runtime referenciado.\nCrea un prefab con NetworkObject + NetworkAvatar y asignalo en FusionSession.",
                "Ok");
        }
    }
}
