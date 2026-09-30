using System.Runtime.InteropServices;
using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Barra de direcciones y portapapeles del navegador (Plugins/WebGL/UrlBridge.jslib).
    /// Application.absoluteURL no cambia tras replaceState, asi que la URL vigente vive aca.
    /// </summary>
    public static class UrlBridge
    {
        public const string EditorPageUrl = "https://juliojulioso.github.io/metaverso-mvp/desktop/";

        static string _current;

        public static string CurrentUrl
        {
            get
            {
                if (_current == null)
                    _current = string.IsNullOrEmpty(Application.absoluteURL) ? EditorPageUrl : Application.absoluteURL;
                return _current;
            }
        }

        /// <summary>Solo editor: simula la URL de entrada (Bootstrap.EditorQuery).</summary>
        public static void SetEditorUrl(string url)
        {
            _current = url;
        }

        /// <summary>Deja ?world= en la barra de direcciones. ?spawn= solo vale para la entrada.</summary>
        public static void SetWorld(string worldId)
        {
            var url = UrlState.WithParam(CurrentUrl, UrlState.WorldKey, worldId);
            _current = UrlState.WithParam(url, UrlState.SpawnKey, null);
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_ReplaceQuery(UrlState.SearchOf(_current));
#endif
        }

        public static string ShareLink(string worldId) => UrlState.ShareUrl(CurrentUrl, worldId);

        public static void CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_CopyText(text);
#else
            GUIUtility.systemCopyBuffer = text;
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void Metaverso_ReplaceQuery(string search);

        [DllImport("__Internal")]
        static extern void Metaverso_CopyText(string text);
#endif
    }
}
