using System;

namespace Metaverso
{
    public enum QualityTier
    {
        Quest = 0,
        Pc = 1,
        Ultra = 2
    }

    public enum DeviceClass
    {
        Desktop,
        Quest,
        Mobile
    }

    public enum WebGraphicsApi
    {
        WebGL2,
        WebGPU
    }

    /// <summary>
    /// Valores de un nivel. QualityTierSetup los escribe en los assets URP y en QualitySettings;
    /// QualityTierController aplica en runtime lo que es por camara o por canvas.
    /// </summary>
    public readonly struct QualityTierProfile
    {
        public readonly string QualityLevel;
        public readonly string PipelineAsset;
        public readonly string Renderer;
        public readonly bool Hdr;
        public readonly int Msaa;
        public readonly float RenderScale;
        public readonly bool PostProcessing;
        public readonly bool Ssao;
        public readonly bool MainLightShadows;
        public readonly int MainShadowResolution;
        public readonly int ShadowCascades;
        public readonly float ShadowDistance;
        public readonly bool SoftShadows;
        public readonly bool AdditionalLightShadows;
        public readonly int AdditionalLightsPerObject;
        public readonly bool ReflectionProbeBlending;
        public readonly float LodBias;
        public readonly int TextureMipLimit;
        public readonly float MaxPixelRatio;

        public QualityTierProfile(
            string qualityLevel, string pipelineAsset, string renderer,
            bool hdr, int msaa, float renderScale, bool postProcessing, bool ssao,
            bool mainLightShadows, int mainShadowResolution, int shadowCascades, float shadowDistance, bool softShadows,
            bool additionalLightShadows, int additionalLightsPerObject, bool reflectionProbeBlending,
            float lodBias, int textureMipLimit, float maxPixelRatio)
        {
            QualityLevel = qualityLevel;
            PipelineAsset = pipelineAsset;
            Renderer = renderer;
            Hdr = hdr;
            Msaa = msaa;
            RenderScale = renderScale;
            PostProcessing = postProcessing;
            Ssao = ssao;
            MainLightShadows = mainLightShadows;
            MainShadowResolution = mainShadowResolution;
            ShadowCascades = shadowCascades;
            ShadowDistance = shadowDistance;
            SoftShadows = softShadows;
            AdditionalLightShadows = additionalLightShadows;
            AdditionalLightsPerObject = additionalLightsPerObject;
            ReflectionProbeBlending = reflectionProbeBlending;
            LodBias = lodBias;
            TextureMipLimit = textureMipLimit;
            MaxPixelRatio = maxPixelRatio;
        }
    }

    /// <summary>
    /// Nivel de calidad por dispositivo y API grafica. Quest siempre en Quest (WebXR solo corre en WebGL2).
    /// PC entra en PC con WebGL2 y en Ultra con WebGPU; ?quality= o el selector del HUD bajan o suben
    /// hasta el techo del dispositivo.
    /// </summary>
    public static class QualityTierRules
    {
        public const string UrlKey = "quality";

        public static readonly string[] QualityLevels = { "Web_Quest", "Web_PC", "Web_Ultra" };

        public static DeviceClass DeviceFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return DeviceClass.Desktop;
            if (Contains(userAgent, "OculusBrowser") || Contains(userAgent, "Quest"))
                return DeviceClass.Quest;
            if (Contains(userAgent, "Android") || Contains(userAgent, "iPhone") || Contains(userAgent, "iPad") || Contains(userAgent, "Mobile"))
                return DeviceClass.Mobile;
            return DeviceClass.Desktop;
        }

        public static QualityTier Ceiling(DeviceClass device)
        {
            switch (device)
            {
                case DeviceClass.Quest:
                    return QualityTier.Quest;
                case DeviceClass.Mobile:
                    return QualityTier.Pc;
                default:
                    return QualityTier.Ultra;
            }
        }

        public static QualityTier Default(DeviceClass device, WebGraphicsApi api)
        {
            if (device != DeviceClass.Desktop)
                return QualityTier.Quest;
            return api == WebGraphicsApi.WebGPU ? QualityTier.Ultra : QualityTier.Pc;
        }

        public static QualityTier Choose(DeviceClass device, WebGraphicsApi api, QualityTier? requested)
        {
            if (!requested.HasValue)
                return Default(device, api);
            return Clamp(requested.Value, device);
        }

        public static QualityTier Clamp(QualityTier tier, DeviceClass device)
        {
            var ceiling = Ceiling(device);
            return tier > ceiling ? ceiling : tier;
        }

        /// <summary>Siguiente nivel del selector: sube hasta el techo y vuelve a Quest.</summary>
        public static QualityTier Next(QualityTier current, QualityTier ceiling)
        {
            return current >= ceiling ? QualityTier.Quest : current + 1;
        }

        public static QualityTier? Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            switch (value.Trim().ToLowerInvariant())
            {
                case "quest":
                case "low":
                    return QualityTier.Quest;
                case "pc":
                case "high":
                    return QualityTier.Pc;
                case "ultra":
                    return QualityTier.Ultra;
                default:
                    return null;
            }
        }

        public static string Id(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.Quest:
                    return "quest";
                case QualityTier.Ultra:
                    return "ultra";
                default:
                    return "pc";
            }
        }

        public static string Label(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.Quest:
                    return "Quest";
                case QualityTier.Ultra:
                    return "Ultra";
                default:
                    return "PC";
            }
        }

        public static string ApiLabel(WebGraphicsApi api)
        {
            return api == WebGraphicsApi.WebGPU ? "WebGPU" : "WebGL2";
        }

        public static QualityTierProfile Profile(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.Quest:
                    return new QualityTierProfile(
                        QualityLevels[0], "Web_Quest_RPAsset", "Web_Quest_Renderer",
                        hdr: false, msaa: 4, renderScale: 1f, postProcessing: false, ssao: false,
                        mainLightShadows: true, mainShadowResolution: 1024, shadowCascades: 1, shadowDistance: 20f, softShadows: false,
                        additionalLightShadows: false, additionalLightsPerObject: 2, reflectionProbeBlending: false,
                        lodBias: 1f, textureMipLimit: 0, maxPixelRatio: 1f);
                case QualityTier.Ultra:
                    return new QualityTierProfile(
                        QualityLevels[2], "Web_Ultra_RPAsset", "Web_PC_Renderer",
                        hdr: true, msaa: 4, renderScale: 1f, postProcessing: true, ssao: true,
                        mainLightShadows: true, mainShadowResolution: 4096, shadowCascades: 4, shadowDistance: 60f, softShadows: true,
                        additionalLightShadows: true, additionalLightsPerObject: 8, reflectionProbeBlending: true,
                        lodBias: 2f, textureMipLimit: 0, maxPixelRatio: 2f);
                default:
                    return new QualityTierProfile(
                        QualityLevels[1], "Web_PC_RPAsset", "Web_PC_Renderer",
                        hdr: true, msaa: 4, renderScale: 1f, postProcessing: true, ssao: true,
                        mainLightShadows: true, mainShadowResolution: 2048, shadowCascades: 2, shadowDistance: 40f, softShadows: true,
                        additionalLightShadows: true, additionalLightsPerObject: 4, reflectionProbeBlending: true,
                        lodBias: 1.5f, textureMipLimit: 0, maxPixelRatio: 1.5f);
            }
        }

        static bool Contains(string text, string value)
        {
            return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
