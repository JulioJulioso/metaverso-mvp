using UnityEngine;

namespace Metaverso
{
    public enum ClientMode
    {
        Desktop,
        Vr
    }

    public struct ModePresentation
    {
        public bool DesktopController;
        public bool DesktopCamera;
        public bool XrCamera;
        public bool XrLocomotion;
        public bool ScreenHud;
        public bool WorldHud;
    }

    /// <summary>
    /// Reglas puras del cambio PC / VR. El controlador XR solo las aplica.
    /// </summary>
    public static class PlatformModeSwitch
    {
        public static ModePresentation For(ClientMode mode)
        {
            if (mode == ClientMode.Vr)
            {
                return new ModePresentation
                {
                    DesktopController = false,
                    DesktopCamera = false,
                    XrCamera = true,
                    XrLocomotion = true,
                    ScreenHud = false,
                    WorldHud = true
                };
            }

            return new ModePresentation
            {
                DesktopController = true,
                DesktopCamera = true,
                XrCamera = false,
                XrLocomotion = false,
                ScreenHud = true,
                WorldHud = false
            };
        }

        public static void TransferPose(
            ClientMode entering,
            Vector3 desktopPosition,
            Quaternion desktopRotation,
            Vector3 xrPosition,
            Quaternion xrRotation,
            out Vector3 outDesktopPosition,
            out Quaternion outDesktopRotation,
            out Vector3 outXrPosition,
            out Quaternion outXrRotation)
        {
            if (entering == ClientMode.Vr)
            {
                outXrPosition = desktopPosition;
                outXrRotation = Quaternion.Euler(0f, desktopRotation.eulerAngles.y, 0f);
                outDesktopPosition = desktopPosition;
                outDesktopRotation = desktopRotation;
                return;
            }

            outDesktopPosition = xrPosition;
            outDesktopRotation = Quaternion.Euler(0f, xrRotation.eulerAngles.y, 0f);
            outXrPosition = xrPosition;
            outXrRotation = xrRotation;
        }
    }

    public static class ClientModeState
    {
        public static ClientMode Current { get; private set; } = ClientMode.Desktop;

        public static void Set(ClientMode mode)
        {
            Current = mode;
        }
    }
}
