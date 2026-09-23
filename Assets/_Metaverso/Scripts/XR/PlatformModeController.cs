using UnityEngine;
using WebXR;

namespace Metaverso.XR
{
    /// <summary>
    /// PC en tercera persona. Si el navegador entra a immersive-vr, pasa al rig de XR
    /// en la misma posicion y al salir vuelve.
    /// En VR el cuerpo de escritorio sigue a la cabeza: monedas, pelota y muros miden
    /// distancia contra el objeto con tag Player.
    /// </summary>
    public class PlatformModeController : MonoBehaviour
    {
        public Behaviour DesktopController;
        public Camera DesktopCamera;
        public Camera XrCamera;
        public Behaviour XrLocomotion;
        public GameObject ScreenHud;
        public GameObject WorldHud;
        public Transform DesktopBody;
        public Transform XrRig;

        WebXRState _last = WebXRState.NORMAL;
        CharacterController _desktopCollider;
        CharacterController _xrCollider;
        Renderer[] _desktopRenderers;

        void Awake()
        {
            if (DesktopBody != null)
            {
                _desktopCollider = DesktopBody.GetComponent<CharacterController>();
                _desktopRenderers = DesktopBody.GetComponentsInChildren<Renderer>(true);
            }
            if (XrRig != null)
                _xrCollider = XrRig.GetComponent<CharacterController>();
        }

        void OnEnable()
        {
            Apply(ClientMode.Desktop, transferPose: false);
        }

        void Update()
        {
            var manager = WebXRManager.Instance;
            if (manager == null || manager.XRState == _last)
                return;
            _last = manager.XRState;
            var mode = _last == WebXRState.VR ? ClientMode.Vr : ClientMode.Desktop;
            Apply(mode, transferPose: mode == ClientMode.Vr);
        }

        void LateUpdate()
        {
            if (ClientModeState.Current != ClientMode.Vr || DesktopBody == null || XrCamera == null || XrRig == null)
                return;
            var head = XrCamera.transform.position;
            DesktopBody.position = new Vector3(head.x, XrRig.position.y, head.z);
            DesktopBody.rotation = Quaternion.Euler(0f, XrCamera.transform.eulerAngles.y, 0f);
        }

        public void Apply(ClientMode mode, bool transferPose)
        {
            if (transferPose && DesktopBody != null && XrRig != null)
            {
                PlatformModeSwitch.TransferPose(
                    mode,
                    DesktopBody.position,
                    DesktopBody.rotation,
                    XrRig.position,
                    XrRig.rotation,
                    out var desktopPosition,
                    out var desktopRotation,
                    out var xrPosition,
                    out var xrRotation);
                DesktopBody.SetPositionAndRotation(desktopPosition, desktopRotation);
                XrRig.SetPositionAndRotation(xrPosition, xrRotation);
            }

            var view = PlatformModeSwitch.For(mode);
            if (DesktopController != null) DesktopController.enabled = view.DesktopController;
            if (_desktopCollider != null) _desktopCollider.enabled = view.DesktopController;
            if (_xrCollider != null) _xrCollider.enabled = view.XrLocomotion;
            if (DesktopCamera != null) DesktopCamera.enabled = view.DesktopCamera;
            if (XrCamera != null) XrCamera.enabled = view.XrCamera;
            if (XrLocomotion != null) XrLocomotion.enabled = view.XrLocomotion;
            if (ScreenHud != null) ScreenHud.SetActive(view.ScreenHud);
            if (WorldHud != null) WorldHud.SetActive(view.WorldHud);

            if (_desktopRenderers != null)
            {
                for (var i = 0; i < _desktopRenderers.Length; i++)
                    _desktopRenderers[i].enabled = view.DesktopCamera;
            }

            var desktopListener = DesktopCamera != null ? DesktopCamera.GetComponent<AudioListener>() : null;
            var xrListener = XrCamera != null ? XrCamera.GetComponent<AudioListener>() : null;
            if (desktopListener != null) desktopListener.enabled = view.DesktopCamera;
            if (xrListener != null) xrListener.enabled = view.XrCamera;

            ClientModeState.Set(mode);
        }
    }
}
