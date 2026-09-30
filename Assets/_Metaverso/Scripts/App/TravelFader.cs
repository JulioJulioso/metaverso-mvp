using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Metaverso
{
    /// <summary>
    /// Fundido a negro con texto de carga en PC. En VR el jugador espera en la losa de Boot
    /// y el texto sale en el HUD de mundo (MetaversoHud.SetStatus).
    /// Arranca opaco: la primera carga ocurre detras del negro.
    /// </summary>
    public class TravelFader : MonoBehaviour
    {
        CanvasGroup _group;
        Text _text;

        public float Alpha => _group != null ? _group.alpha : 0f;

        void Awake()
        {
            Build();
        }

        public void SetText(string text)
        {
            if (_text != null)
                _text.text = text ?? "";
        }

        public IEnumerator FadeTo(float alpha, float seconds)
        {
            if (_group == null)
                yield break;
            _group.blocksRaycasts = true;
            var start = _group.alpha;
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, alpha, Mathf.Clamp01(elapsed / seconds));
                yield return null;
            }

            _group.alpha = alpha;
            _group.blocksRaycasts = alpha > 0.01f;
        }

        void Build()
        {
            var canvasGo = new GameObject("FaderCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();
            _group = canvasGo.AddComponent<CanvasGroup>();
            _group.alpha = 1f;

            var backGo = new GameObject("Black");
            backGo.transform.SetParent(canvasGo.transform, false);
            var back = backGo.AddComponent<Image>();
            back.color = new Color(0.06f, 0.055f, 0.05f, 1f);
            var backRect = back.rectTransform;
            backRect.anchorMin = Vector2.zero;
            backRect.anchorMax = Vector2.one;
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(canvasGo.transform, false);
            _text = textGo.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontSize = 30;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.color = new Color(0.96f, 0.94f, 0.9f, 1f);
            _text.text = "Cargando";
            var textRect = _text.rectTransform;
            textRect.anchorMin = new Vector2(0.1f, 0.4f);
            textRect.anchorMax = new Vector2(0.9f, 0.6f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
