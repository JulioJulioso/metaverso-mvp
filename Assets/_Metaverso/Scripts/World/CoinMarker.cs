using UnityEngine;

namespace Metaverso
{
    public class CoinMarker : MonoBehaviour
    {
        public float CollectRadius = 0.9f;
        public CoinGate Gate;
        public MetaversoHud Hud;

        bool _collected;
        Transform _player;

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _player = player.transform;
        }

        void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            if (_collected || _player == null)
                return;

            var flat = transform.position - _player.position;
            flat.y = 0f;
            if (flat.magnitude > CollectRadius)
                return;

            _collected = true;
            gameObject.SetActive(false);
            Gate?.NotifyCollected();
            Hud?.ShowMessage("Marcador recogido");
        }
    }
}
