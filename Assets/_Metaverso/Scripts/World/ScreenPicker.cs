using UnityEngine;
using UnityEngine.InputSystem;

namespace Metaverso
{
    /// <summary>
    /// Click de PC. OnMouseDown no dispara si el proyecto usa solo el Input System nuevo.
    /// </summary>
    public class ScreenPicker : MonoBehaviour
    {
        public MetaversoHud Hud;

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;
            if (Cursor.lockState == CursorLockMode.Locked)
                return;

            var camera = GetComponent<Camera>();
            if (camera == null)
                camera = Camera.main;
            if (camera == null)
                return;

            var ray = camera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 80f))
                return;

            var media = hit.collider.GetComponentInParent<MediaScreen>();
            if (media != null)
            {
                media.Open();
                return;
            }

            var label = hit.collider.GetComponentInParent<BimLabel>();
            if (label != null)
                Hud?.ShowMessage(label.Describe());
        }
    }
}
