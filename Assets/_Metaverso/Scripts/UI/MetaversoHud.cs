using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Metaverso
{
    /// <summary>
    /// HUD de pantalla en PC y de mundo en VR. La UI se construye en Awake porque los
    /// listeners de botones agregados por codigo no se guardan en la escena.
    /// </summary>
    public class MetaversoHud : MonoBehaviour
    {
        public bool WorldSpace;
        public CircuitTracker Circuit;
        public WallAssembly Walls;

        static readonly List<MetaversoHud> Instances = new List<MetaversoHud>();
        static string _lastMessage;

        Text _version;
        Text _message;
        Text _checklist;
        GameObject _wallBar;
        GameObject _messagePanel;
        bool _built;

        public static MetaversoHud Create(string name, bool worldSpace)
        {
            var root = new GameObject(name);
            var hud = root.AddComponent<MetaversoHud>();
            hud.WorldSpace = worldSpace;
            return hud;
        }

        void Awake()
        {
            Build();
            Instances.Add(this);
        }

        void OnEnable()
        {
            if (_built && !string.IsNullOrEmpty(_lastMessage))
                SetMessage(_lastMessage);
        }

        void OnDestroy()
        {
            Instances.Remove(this);
            if (Circuit != null)
                Circuit.Changed -= RefreshChecklist;
        }

        public void Bind(CircuitTracker circuit, WallAssembly walls)
        {
            if (Circuit != null)
                Circuit.Changed -= RefreshChecklist;
            Circuit = circuit;
            Walls = walls;
            if (Circuit != null)
                Circuit.Changed += RefreshChecklist;
            RefreshChecklist();
        }

        public void SetWallActionsVisible(bool visible)
        {
            if (_wallBar != null)
                _wallBar.SetActive(visible);
        }

        /// <summary>Muestra el mensaje en todos los HUD (PC y VR).</summary>
        public void ShowMessage(string message)
        {
            _lastMessage = message;
            for (var i = 0; i < Instances.Count; i++)
                Instances[i].SetMessage(message);
        }

        void SetMessage(string message)
        {
            if (_message == null)
                return;
            _message.text = message;
            if (_messagePanel != null)
                _messagePanel.SetActive(true);
        }

        void Build()
        {
            if (_built)
                return;
            _built = true;

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                var rect = canvas.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(900f, 520f);
                rect.localScale = Vector3.one * 0.0016f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            _version = MakeText(canvas.transform, "Version", "v0.1.0", 18, TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 12f), new Vector2(420f, 32f));
            var info = Resources.Load<BuildInfo>("BuildInfo");
            if (info != null)
                _version.text = info.Label;

            _checklist = MakeText(canvas.transform, "Checklist", "Circuito", 20, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(520f, 280f));

            _wallBar = new GameObject("WallActions");
            _wallBar.transform.SetParent(canvas.transform, false);
            var bar = _wallBar.AddComponent<RectTransform>();
            bar.anchorMin = new Vector2(0.5f, 0f);
            bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.anchoredPosition = new Vector2(0f, 72f);
            bar.sizeDelta = new Vector2(460f, 48f);
            MakeButton(_wallBar.transform, "Levantar muros", new Vector2(-110f, 0f), () => Walls?.PlayRise());
            MakeButton(_wallBar.transform, "Despiece", new Vector2(110f, 0f), () => Walls?.ToggleExplode());
            _wallBar.SetActive(false);

            _messagePanel = new GameObject("Message");
            _messagePanel.transform.SetParent(canvas.transform, false);
            var panel = _messagePanel.AddComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(640f, 120f);
            var bg = _messagePanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.06f, 0.86f);
            _message = MakeText(_messagePanel.transform, "Body", "", 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var messageRect = _message.rectTransform;
            messageRect.offsetMin = new Vector2(16f, 36f);
            messageRect.offsetMax = new Vector2(-16f, -8f);
            MakeButton(_messagePanel.transform, "Aceptar", new Vector2(0f, -42f), () => _messagePanel.SetActive(false));
            _messagePanel.SetActive(false);

            var help = MakeText(canvas.transform, "Help",
                "PC: WASD, mouse, Espacio salta, E toma, F suelta, click en la pantalla.\nVR: stick izquierdo camina, stick derecho gira, gatillo toma la pelota.",
                16, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 12f), new Vector2(760f, 64f));
            help.color = new Color(1f, 1f, 1f, 0.8f);
            RefreshChecklist();
        }

        void RefreshChecklist()
        {
            if (_checklist == null || Circuit == null)
                return;
            var text = $"Circuito {Circuit.CompletedCount}/{Circuit.Steps.Count}\n";
            for (var i = 0; i < Circuit.Steps.Count; i++)
            {
                var id = Circuit.Steps[i];
                text += (Circuit.IsDone(id) ? "[x] " : "[ ] ") + Circuit.LabelFor(id) + "\n";
            }
            _checklist.text = text;
        }

        void Update()
        {
            if (Walls == null)
                return;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            var near = Walls.IsPlayerNear(player.transform.position);
            if (ClientModeState.Current == ClientMode.Vr)
                near = near || Walls.IsPlayerNear(Camera.main != null ? Camera.main.transform.position : player.transform.position);
            SetWallActionsVisible(near);
        }

        static Text MakeText(Transform parent, string name, string value, int size, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = value;
            var rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = pos;
            rect.sizeDelta = sizeDelta;
            return text;
        }

        static void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.75f, 0.62f, 0.42f, 0.95f);
            var button = go.AddComponent<Button>();
            button.onClick.AddListener(onClick);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 36f);
            rect.anchoredPosition = pos;
            var text = MakeText(go.transform, "Label", label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.color = new Color(0.12f, 0.1f, 0.08f, 1f);
            var textRect = text.rectTransform;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
