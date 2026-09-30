using UnityEditor;
using UnityEditor.SceneManagement;

namespace Metaverso.EditorTools
{
    /// <summary>
    /// Play siempre arranca en Boot, como el build. Si la escena abierta es un mundo
    /// (Scenes/Worlds/{id}.unity), Boot entra directo a ese mundo; si no, al default.
    /// El mundo se carga desde disco: guarda antes de dar Play.
    /// </summary>
    [InitializeOnLoad]
    static class PlayFromBoot
    {
        const string PrefKey = "Metaverso.PlayFromBoot";
        const string MenuPath = "Metaverso/Mundos/Play siempre desde Boot";

        static PlayFromBoot()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static bool Enabled => EditorPrefs.GetBool(PrefKey, true);

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            EditorPrefs.SetBool(PrefKey, !Enabled);
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            // El Test Runner entra a Play desde una escena temporal sin ruta en Assets/.
            var active = EditorSceneManager.GetActiveScene().path;
            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(WorldPaths.BootScene);
            if (!Enabled || boot == null || string.IsNullOrEmpty(active) || !active.StartsWith("Assets/"))
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            SessionState.SetString(Bootstrap.EditorWorldKey, WorldPaths.WorldIdOf(active) ?? "");
            EditorSceneManager.playModeStartScene = boot;
        }
    }
}
