using UnityEngine;
using UnityEngine.InputSystem;

namespace Metaverso
{
    /// <summary>
    /// Orbita en tercera persona. El click izquierdo captura el mouse; Escape lo suelta.
    /// El yaw vive en el jugador para que WASD sea relativo a la camara.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 4.2f;
        public float Height = 1.55f;
        public float Sensitivity = 0.12f;
        public float MinPitch = -25f;
        public float MaxPitch = 55f;

        float _pitch = 12f;

        public float FaceYaw => Target != null ? Target.eulerAngles.y : 0f;

        void Start()
        {
            if (Target != null)
                _pitch = 12f;
        }

        void LateUpdate()
        {
            if (Target == null)
                return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                Cursor.lockState = CursorLockMode.Locked;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                Cursor.lockState = CursorLockMode.None;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                var delta = mouse.delta.ReadValue();
                Target.Rotate(0f, delta.x * Sensitivity, 0f, Space.World);
                _pitch = Mathf.Clamp(_pitch - delta.y * Sensitivity, MinPitch, MaxPitch);
            }

            var yaw = Target.eulerAngles.y;
            var rotation = Quaternion.Euler(_pitch, yaw, 0f);
            var focus = Target.position + Vector3.up * Height;
            transform.position = focus - rotation * Vector3.forward * Distance;
            transform.rotation = rotation;
        }
    }
}
