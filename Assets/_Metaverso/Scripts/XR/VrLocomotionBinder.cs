using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Metaverso.XR
{
    /// <summary>
    /// Conecta los sticks del Quest a los proveedores de locomocion de XR Interaction Toolkit 3.
    /// Stick izquierdo: caminar. Stick derecho: giro por pasos de 30 grados.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class VrLocomotionBinder : MonoBehaviour
    {
        public Transform ForwardSource;
        public ContinuousMoveProvider Move;
        public SnapTurnProvider Turn;

        InputAction _moveAction;
        InputAction _turnAction;

        void Awake()
        {
            if (Move == null)
                Move = GetComponent<ContinuousMoveProvider>();
            if (Turn == null)
                Turn = GetComponent<SnapTurnProvider>();

            _moveAction = Stick("Move", "<XRController>{LeftHand}/thumbstick");
            _turnAction = Stick("Turn", "<XRController>{RightHand}/thumbstick");

            if (Move != null)
            {
                Move.leftHandMoveInput = Reader("Move", _moveAction);
                Move.rightHandMoveInput = Unused();
                Move.enableStrafe = true;
                Move.enableFly = false;
                Move.moveSpeed = 3.2f;
                if (ForwardSource != null)
                    Move.forwardSource = ForwardSource;
            }

            if (Turn != null)
            {
                Turn.leftHandTurnInput = Unused();
                Turn.rightHandTurnInput = Reader("Turn", _turnAction);
                Turn.turnAmount = 30f;
                Turn.enableTurnLeftRight = true;
                Turn.enableTurnAround = true;
            }
        }

        void OnDestroy()
        {
            _moveAction?.Dispose();
            _turnAction?.Dispose();
        }

        static InputAction Stick(string name, string binding)
        {
            var action = new InputAction(name, InputActionType.Value, binding, expectedControlType: "Vector2");
            action.Enable();
            return action;
        }

        static XRInputValueReader<Vector2> Reader(string name, InputAction action)
        {
            var reader = new XRInputValueReader<Vector2>(name, XRInputValueReader.InputSourceMode.InputAction);
            reader.inputAction = action;
            return reader;
        }

        static XRInputValueReader<Vector2> Unused()
        {
            return new XRInputValueReader<Vector2>("unused", XRInputValueReader.InputSourceMode.Unused);
        }
    }
}
