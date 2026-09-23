using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Metaverso.Tests
{
    public class ModeSwitchPlayModeTests
    {
        [UnityTest]
        public IEnumerator EnteringVr_CopiesDesktopPoseOntoRig()
        {
            var desktop = new GameObject("desktop");
            var xr = new GameObject("xr");
            desktop.transform.SetPositionAndRotation(new Vector3(2f, 0f, 4f), Quaternion.Euler(0f, 90f, 0f));

            PlatformModeSwitch.TransferPose(
                ClientMode.Vr,
                desktop.transform.position,
                desktop.transform.rotation,
                xr.transform.position,
                xr.transform.rotation,
                out _,
                out _,
                out var xrPosition,
                out var xrRotation);

            Assert.AreEqual(2f, xrPosition.x, 0.001f);
            Assert.AreEqual(4f, xrPosition.z, 0.001f);
            Assert.AreEqual(90f, xrRotation.eulerAngles.y, 0.1f);

            Object.Destroy(desktop);
            Object.Destroy(xr);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LeavingVr_CopiesRigPoseBackToDesktop()
        {
            PlatformModeSwitch.TransferPose(
                ClientMode.Desktop,
                Vector3.zero,
                Quaternion.identity,
                new Vector3(-3f, 0f, 1f),
                Quaternion.Euler(0f, 180f, 0f),
                out var desktopPosition,
                out var desktopRotation,
                out _,
                out _);

            Assert.AreEqual(-3f, desktopPosition.x, 0.001f);
            Assert.AreEqual(180f, desktopRotation.eulerAngles.y, 0.1f);
            yield return null;
        }
    }
}
