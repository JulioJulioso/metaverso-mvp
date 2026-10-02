using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Metaverso
{
    /// <summary>
    /// Aplica el nivel de QualityTierRules: el nivel de QualitySettings (trae su asset URP),
    /// post-proceso por camara y tope de pixel ratio del canvas. Bootstrap llama a Detect con la URL de entrada.
    /// </summary>
    public class QualityTierController : MonoBehaviour
    {
        public const string PrefKey = "Metaverso.Quality";

        public static QualityTierController Instance { get; private set; }

        public QualityTier Tier { get; private set; } = QualityTier.Pc;
        public QualityTier Ceiling { get; private set; } = QualityTier.Ultra;
        public DeviceClass Device { get; private set; }
        public WebGraphicsApi Api { get; private set; }
        public bool CanChoose => Ceiling > QualityTier.Quest;

#if UNITY_EDITOR
        int _editorLevel = -1;
#endif

        public static QualityTierController Ensure(GameObject host)
        {
            if (Instance != null)
                return Instance;
            var controller = host.GetComponent<QualityTierController>();
            return controller != null ? controller : host.AddComponent<QualityTierController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
#if UNITY_EDITOR
            _editorLevel = QualitySettings.GetQualityLevel();
#endif
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance != this)
                return;
            Instance = null;
#if UNITY_EDITOR
            // En el editor el nivel elegido en Play quedaria guardado en QualitySettings.
            if (_editorLevel >= 0)
                QualitySettings.SetQualityLevel(_editorLevel, true);
#endif
        }

        public void Detect(string url)
        {
            Device = QualityTierRules.DeviceFromUserAgent(UserAgent());
            Api = SystemInfo.graphicsDeviceType == GraphicsDeviceType.WebGPU ? WebGraphicsApi.WebGPU : WebGraphicsApi.WebGL2;
            Ceiling = QualityTierRules.Ceiling(Device);

            var requested = QualityTierRules.Parse(UrlState.ReadParam(url, QualityTierRules.UrlKey))
                ?? QualityTierRules.Parse(PlayerPrefs.GetString(PrefKey, ""));
            var tier = QualityTierRules.Choose(Device, Api, requested);
            Debug.Log($"[Metaverso] Calidad {QualityTierRules.Label(tier)}: dispositivo {Device}, {QualityTierRules.ApiLabel(Api)}, pedido '{(requested.HasValue ? QualityTierRules.Id(requested.Value) : "auto")}'.");
            Apply(tier);
        }

        /// <summary>Eleccion del usuario: se guarda y gana sobre el automatico en las proximas visitas.</summary>
        public void Select(QualityTier tier)
        {
            tier = QualityTierRules.Clamp(tier, Device);
            PlayerPrefs.SetString(PrefKey, QualityTierRules.Id(tier));
            PlayerPrefs.Save();
            Apply(tier);
        }

        public void Cycle()
        {
            Select(QualityTierRules.Next(Tier, Ceiling));
        }

        void Apply(QualityTier tier)
        {
            var profile = QualityTierRules.Profile(tier);
            var level = Array.IndexOf(QualitySettings.names, profile.QualityLevel);
            if (level < 0)
                Debug.LogWarning($"[Metaverso] Falta el nivel de calidad {profile.QualityLevel}. Usa Metaverso > Rendering > Configurar niveles de calidad.");
            else if (level != QualitySettings.GetQualityLevel())
                QualitySettings.SetQualityLevel(level, true);

            Tier = tier;
            ApplyCameras(profile);
            SetPixelRatioCap(profile.MaxPixelRatio);
            ReportGraphics(QualityTierRules.ApiLabel(Api), QualityTierRules.Id(tier));
            MetaversoHud.SetQuality($"{QualityTierRules.Label(tier)} {QualityTierRules.ApiLabel(Api)}", CanChoose ? "Calidad: " + QualityTierRules.Label(tier) : null);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCameras(QualityTierRules.Profile(Tier));
        }

        static void ApplyCameras(QualityTierProfile profile)
        {
            var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera.cameraType != CameraType.Game)
                    continue;
                camera.allowHDR = profile.Hdr;
                camera.allowMSAA = profile.Msaa > 1;
                var data = camera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = profile.PostProcessing;
                data.antialiasing = AntialiasingMode.None;
                data.dithering = profile.PostProcessing;
            }
        }

        static string UserAgent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Metaverso_UserAgent();
#else
            return "";
#endif
        }

        static void SetPixelRatioCap(float cap)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_SetPixelRatioCap(cap);
#endif
        }

        static void ReportGraphics(string api, string tier)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_ReportGraphics(api, tier);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern string Metaverso_UserAgent();

        [DllImport("__Internal")]
        static extern void Metaverso_SetPixelRatioCap(float cap);

        [DllImport("__Internal")]
        static extern void Metaverso_ReportGraphics(string api, string tier);
#endif
    }
}
