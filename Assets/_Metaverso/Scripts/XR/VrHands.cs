using UnityEngine;
using UnityEngine.InputSystem;

namespace Metaverso.XR
{
    /// <summary>
    /// Toma y suelta la pelota con el gatillo, y un teletransporte corto con el click del stick derecho.
    /// XR Interaction Toolkit queda en el rig para grab y rayo; esto cubre el caso en que
    /// sus acciones de seleccion todavia no estan asignadas.
    /// </summary>
    public class VrHands : MonoBehaviour
    {
        public Transform Rig;
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public float GrabRadius = 0.45f;
        public float TeleportDistance = 8f;

        InputAction _grab;
        InputAction _drop;
        InputAction _teleport;
        PickupBall _held;

        void Awake()
        {
            _grab = Button("Grab", "<XRController>{RightHand}/triggerPressed", "<XRController>{LeftHand}/triggerPressed");
            _drop = Button("Drop", "<XRController>{RightHand}/gripPressed", "<XRController>{LeftHand}/gripPressed");
            _teleport = Button("Teleport", "<XRController>{RightHand}/thumbstickClicked");
        }

        void OnDestroy()
        {
            _grab?.Dispose();
            _drop?.Dispose();
            _teleport?.Dispose();
        }

        void Update()
        {
            if (ClientModeState.Current != ClientMode.Vr)
                return;

            if (_grab != null && _grab.WasPressedThisFrame())
                TryGrab();
            if (_drop != null && _drop.WasPressedThisFrame())
                Release();
            if (_teleport != null && _teleport.WasPressedThisFrame())
                Teleport();
        }

        void TryGrab()
        {
            if (_held != null)
                return;
            var balls = FindObjectsByType<PickupBall>(FindObjectsSortMode.None);
            PickupBall best = null;
            var bestDistance = GrabRadius;
            var hands = new[] { RightHand, LeftHand };
            for (var h = 0; h < hands.Length; h++)
            {
                if (hands[h] == null)
                    continue;
                for (var i = 0; i < balls.Length; i++)
                {
                    if (balls[i].IsHeld)
                        continue;
                    var distance = Vector3.Distance(hands[h].position, balls[i].transform.position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = balls[i];
                    }
                }
            }

            if (best == null || RightHand == null)
                return;
            best.Attach(RightHand);
            _held = best;
        }

        void Release()
        {
            if (_held == null)
                return;
            _held.Release();
            _held = null;
        }

        void Teleport()
        {
            if (Rig == null || Head == null)
                return;
            var origin = Head.position;
            var direction = Vector3.ProjectOnPlane(Head.forward, Vector3.up).normalized;
            if (direction.sqrMagnitude < 0.01f)
                return;
            if (!Physics.Raycast(origin, direction, out var hit, TeleportDistance))
                return;
            var delta = hit.point - Head.position;
            delta.y = 0f;
            Rig.position += delta;
        }

        static InputAction Button(string name, params string[] bindings)
        {
            var action = new InputAction(name, InputActionType.Button);
            for (var i = 0; i < bindings.Length; i++)
                action.AddBinding(bindings[i]);
            action.Enable();
            return action;
        }
    }
}
