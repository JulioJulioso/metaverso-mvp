using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;

namespace Metaverso.EditorXR
{
    public static class PlayerRigBuilder
    {
        [MenuItem("Metaverso/Crear rig VR")]
        public static void Create()
        {
            var existing = GameObject.Find("XR Origin");
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);

            var origin = new GameObject("XR Origin");
            var xrOrigin = origin.AddComponent<XROrigin>();
            origin.AddComponent<CharacterController>().height = 1.75f;
            origin.GetComponent<CharacterController>().radius = 0.28f;
            origin.GetComponent<CharacterController>().center = new Vector3(0f, 0.9f, 0f);
            origin.AddComponent<XRBodyTransformer>();
            origin.AddComponent<LocomotionMediator>();
            var move = origin.AddComponent<ContinuousMoveProvider>();
            var turn = origin.AddComponent<SnapTurnProvider>();
            origin.AddComponent<TeleportationProvider>();
            TryAdd(origin, "UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity.GravityProvider, Unity.XR.Interaction.Toolkit");

            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(origin.transform, false);
            var cameraGo = new GameObject("XRCamera");
            cameraGo.transform.SetParent(offset.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.enabled = false;
            var listener = cameraGo.AddComponent<AudioListener>();
            listener.enabled = false;
            cameraGo.AddComponent<TrackedPoseDriver>();
            xrOrigin.Camera = camera;
            xrOrigin.CameraFloorOffsetObject = offset;

            var manager = origin.AddComponent<XRInteractionManager>();
            var left = Hand(origin.transform, "LeftHand", new Vector3(-0.2f, 1.2f, 0.3f), manager);
            var right = Hand(origin.transform, "RightHand", new Vector3(0.2f, 1.2f, 0.3f), manager);

            var binder = origin.AddComponent<Metaverso.XR.VrLocomotionBinder>();
            binder.Move = move;
            binder.Turn = turn;
            binder.ForwardSource = cameraGo.transform;
            var hands = origin.AddComponent<Metaverso.XR.VrHands>();
            hands.Rig = origin.transform;
            hands.Head = cameraGo.transform;
            hands.LeftHand = left.transform;
            hands.RightHand = right.transform;

            var pose = origin.AddComponent<PoseSource>();
            pose.Head = cameraGo.transform;
            pose.LeftHand = left.transform;
            pose.RightHand = right.transform;
            pose.IsVr = true;

            var worldHud = MetaversoHud.Create("HUD-World", true);
            worldHud.transform.SetParent(cameraGo.transform, false);
            worldHud.transform.localPosition = new Vector3(0f, -0.2f, 2f);
            worldHud.transform.localRotation = Quaternion.identity;
            worldHud.gameObject.SetActive(false);

            var desktop = GameObject.Find("Player");
            var desktopCamera = GameObject.Find("DesktopCamera");
            var screenHud = GameObject.Find("HUD-Screen");
            var mode = origin.AddComponent<Metaverso.XR.PlatformModeController>();
            mode.DesktopBody = desktop != null ? desktop.transform : null;
            mode.DesktopController = desktop != null ? desktop.GetComponent<DesktopPlayerController>() : null;
            mode.DesktopCamera = desktopCamera != null ? desktopCamera.GetComponent<Camera>() : null;
            mode.XrRig = origin.transform;
            mode.XrCamera = camera;
            mode.XrLocomotion = move;
            mode.ScreenHud = screenHud;
            mode.WorldHud = worldHud.gameObject;

            TryAddTeleportAreas();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Metaverso] Rig VR creado. Entra con el boton VR de la plantilla WebXR.");
        }

        static GameObject Hand(Transform parent, string name, Vector3 localPosition, XRInteractionManager manager)
        {
            var hand = new GameObject(name);
            hand.transform.SetParent(parent, false);
            hand.transform.localPosition = localPosition;
            hand.AddComponent<TrackedPoseDriver>();
            var direct = hand.AddComponent<XRDirectInteractor>();
            direct.interactionManager = manager;

            var rayGo = new GameObject(name + "Ray");
            rayGo.transform.SetParent(hand.transform, false);
            var ray = rayGo.AddComponent<XRRayInteractor>();
            ray.interactionManager = manager;
            return hand;
        }

        static void TryAdd(GameObject host, string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null || type.IsAbstract || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.Log("[Metaverso] Componente opcional no encontrado: " + typeName);
                return;
            }
            host.AddComponent(type);
        }

        static void TryAddTeleportAreas()
        {
            var type = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea, Unity.XR.Interaction.Toolkit");
            if (type == null)
                return;
            var colliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            for (var i = 0; i < colliders.Length; i++)
            {
                var host = colliders[i].gameObject;
                if (host.GetComponent<PickupBall>() != null || host.GetComponent<MediaScreen>() != null)
                    continue;
                if (!host.name.StartsWith("pad-") && host.name != "Ground")
                    continue;
                if (host.GetComponent(type) == null)
                    host.AddComponent(type);
            }
        }
    }
}
