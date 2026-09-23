using UnityEngine;
using UnityEngine.InputSystem;

namespace Metaverso
{
    public interface IPlayerInteractable
    {
        bool CanInteract(Vector3 playerPosition);
        void Interact(DesktopPlayerController player);
        void Drop(DesktopPlayerController player);
    }

    [RequireComponent(typeof(CharacterController))]
    public class DesktopPlayerController : MonoBehaviour
    {
        public float MoveSpeed = 4.2f;
        public float JumpSpeed = 6.2f;
        public float Gravity = 18f;
        public Transform HoldPoint;

        CharacterController _body;
        float _vertical;

        public bool IsGrounded => _body != null && _body.isGrounded;
        public Transform Hold => HoldPoint;

        void Awake()
        {
            _body = GetComponent<CharacterController>();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var move = Vector2.zero;
            var jump = false;
            var interact = false;
            var drop = false;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
                jump = keyboard.spaceKey.wasPressedThisFrame;
                interact = keyboard.eKey.wasPressedThisFrame;
                drop = keyboard.fKey.wasPressedThisFrame;
            }

            var yaw = transform.rotation;
            var wish = yaw * new Vector3(move.x, 0f, move.y);
            if (wish.sqrMagnitude > 1f)
                wish.Normalize();

            if (_body.isGrounded && _vertical < 0f)
                _vertical = -1f;
            if (jump && _body.isGrounded)
                _vertical = JumpSpeed;
            _vertical -= Gravity * Time.deltaTime;

            _body.Move((wish * MoveSpeed + Vector3.up * _vertical) * Time.deltaTime);

            if (interact)
                TryInteract();
            if (drop)
                TryDrop();
        }

        void TryInteract()
        {
            var all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            IPlayerInteractable best = null;
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i] is not IPlayerInteractable interactable)
                    continue;
                if (!interactable.CanInteract(transform.position))
                    continue;
                best = interactable;
                break;
            }

            best?.Interact(this);
        }

        void TryDrop()
        {
            var all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i] is IPlayerInteractable interactable)
                    interactable.Drop(this);
            }
        }
    }
}
