using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Lleva a otro mundo del catalogo. Mide distancia contra el objeto con tag Player (en VR
    /// sigue a la cabeza), igual que el resto del mundo, asi funciona sin fisica de triggers.
    /// Al acercarse precarga los bundles del destino. Solo cruza si el jugador estuvo fuera
    /// una vez: quien aparece encima de un portal no rebota.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        public string DestinationWorld = "lobby";
        public string DestinationSpawn;
        public float Radius = 0.9f;
        public float Height = 2.6f;
        public float PreloadRadius = 10f;
        public Renderer Surface;
        public Color Tint = new Color(0.35f, 0.75f, 1f);

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        Transform _player;
        Transform _label;
        TextMesh _labelText;
        MaterialPropertyBlock _block;
        bool _armed;
        bool _preloaded;

        void Start()
        {
            BuildLabel();
            RefreshLabel();
        }

        void Update()
        {
            if (_player == null)
            {
                var found = GameObject.FindGameObjectWithTag("Player");
                if (found == null)
                    return;
                _player = found.transform;
            }

            var travel = WorldTravel.Instance;
            var position = _player.position;
            var local = transform.InverseTransformPoint(position);
            var inside = new Vector2(local.x, local.z).magnitude <= Radius && local.y > -0.5f && local.y < Height;

            if (!inside)
                _armed = true;
            else if (_armed && travel != null && !travel.IsBusy)
            {
                _armed = false;
                travel.Go(DestinationWorld, DestinationSpawn);
            }

            if (!_preloaded && travel != null && Vector3.Distance(position, transform.position) <= PreloadRadius)
            {
                _preloaded = true;
                travel.Preload(DestinationWorld);
            }

            Pulse();
        }

        void LateUpdate()
        {
            var camera = Camera.main;
            if (_label == null || camera == null)
                return;
            var away = _label.position - camera.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.001f)
                _label.rotation = Quaternion.LookRotation(away, Vector3.up);
        }

        void Pulse()
        {
            if (Surface == null)
                return;
            _block ??= new MaterialPropertyBlock();
            var wave = 0.75f + 0.25f * Mathf.Sin(Time.time * 2.4f);
            var color = Tint * wave;
            color.a = 0.85f;
            Surface.GetPropertyBlock(_block);
            _block.SetColor(BaseColor, color);
            Surface.SetPropertyBlock(_block);
        }

        void BuildLabel()
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, Height + 0.45f, 0f);
            _label = go.transform;
            _labelText = go.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _labelText.font = font;
            _labelText.fontSize = 64;
            _labelText.characterSize = 0.05f;
            _labelText.anchor = TextAnchor.MiddleCenter;
            _labelText.alignment = TextAlignment.Center;
            _labelText.color = Color.white;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        void RefreshLabel()
        {
            if (_labelText == null)
                return;
            var entry = WorldTravel.Instance != null ? WorldTravel.Instance.Catalog?.Find(DestinationWorld) : null;
            var name = entry != null ? entry.DisplayName : DestinationWorld;
            if (entry != null && entry.IsPrivate)
                name += "\n(privado)";
            _labelText.text = name;
        }
    }
}
