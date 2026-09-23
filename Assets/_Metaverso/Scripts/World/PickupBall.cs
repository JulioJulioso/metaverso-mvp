using UnityEngine;

namespace Metaverso
{
    [RequireComponent(typeof(Rigidbody))]
    public class PickupBall : MonoBehaviour, IPlayerInteractable
    {
        public float InteractRadius = 1.25f;
        public MetaversoHud Hud;

        Rigidbody _body;
        Transform _holder;

        public bool IsHeld => _holder != null;
        public bool Grounded { get; private set; }

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        void OnCollisionStay(Collision collision)
        {
            Grounded = true;
        }

        void OnCollisionExit(Collision collision)
        {
            Grounded = false;
        }

        public bool CanInteract(Vector3 playerPosition)
        {
            if (IsHeld)
                return false;
            var flat = transform.position - playerPosition;
            flat.y = 0f;
            return flat.magnitude <= InteractRadius;
        }

        public void Interact(DesktopPlayerController player)
        {
            if (player == null || player.Hold == null)
                return;
            Attach(player.Hold);
            Hud?.ShowMessage("Pelota recogida. F para soltar.");
        }

        public void Drop(DesktopPlayerController player)
        {
            if (!IsHeld)
                return;
            Release();
            Hud?.ShowMessage("Pelota soltada.");
        }

        public void Attach(Transform holder)
        {
            if (holder == null || IsHeld)
                return;
            _holder = holder;
            _body.isKinematic = true;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            transform.SetParent(holder, false);
            transform.localPosition = Vector3.zero;
            Grounded = false;
        }

        public void Release()
        {
            if (!IsHeld)
                return;
            transform.SetParent(null, true);
            _body.isKinematic = false;
            _holder = null;
        }
    }
}
