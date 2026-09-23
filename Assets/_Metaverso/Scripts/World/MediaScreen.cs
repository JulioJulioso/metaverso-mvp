using System.Runtime.InteropServices;
using UnityEngine;

namespace Metaverso
{
    public class MediaScreen : MonoBehaviour
    {
        public string EmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ";
        public string Title = "Referencia de proyecto (demo)";
        public MetaversoHud Hud;

        public void Open()
        {
            if (ClientModeState.Current == ClientMode.Vr)
            {
                Hud?.ShowMessage($"Video: {Title}. En VR queda como referencia; se abre en el navegador al salir.");
                return;
            }

            MediaOverlay.Show(EmbedUrl);
        }

        void OnMouseDown()
        {
            Open();
        }
    }

    public static class MediaOverlay
    {
        public static void Show(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_ShowVideo(url);
#else
            Application.OpenURL(url);
#endif
        }

        public static void Hide()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Metaverso_HideVideo();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void Metaverso_ShowVideo(string url);

        [DllImport("__Internal")]
        static extern void Metaverso_HideVideo();
#endif
    }
}
