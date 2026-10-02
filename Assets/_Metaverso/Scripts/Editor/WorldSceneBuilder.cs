using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Metaverso.EditorTools
{
    /// <summary>
    /// Genera Boot (rig persistente, unica escena del build) y los mundos de blockout.
    /// Lobby y Pabellon A son placeholders hasta que llegue el arte.
    /// </summary>
    public static class WorldSceneBuilder
    {
        public const float HoldingDepth = -500f;

        [MenuItem("Metaverso/Mundos/Generar Boot y mundos")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Metaverso", "Sal de Play antes de generar escenas.", "Ok");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Directory.CreateDirectory(WorldPaths.WorldsFolder);
            BuildLobby();
            BuildPabellonA();
            TestWorldBuilder.CreateOrReplace();
            if (File.Exists(WorldPaths.LegacyCircuitoScene))
                AssetDatabase.DeleteAsset(WorldPaths.LegacyCircuitoScene);
            BuildBoot();

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(WorldPaths.BootScene, true) };
            var addresses = WorldAddressables.Configure();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Metaverso] Boot + {addresses.Count} mundos listos. Play arranca desde Boot con la escena de mundo abierta.");
        }

        [MenuItem("Metaverso/Mundos/Regenerar Boot")]
        public static void BuildBoot()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var holding = new Vector3(0f, HoldingDepth, 0f);

            var root = new GameObject("Rig");
            var rig = root.AddComponent<PersistentRig>();

            var pad = new GameObject("HoldingPad");
            pad.transform.SetParent(root.transform, false);
            pad.transform.position = holding;
            var padCollider = pad.AddComponent<BoxCollider>();
            padCollider.center = new Vector3(0f, -0.5f, 0f);
            padCollider.size = new Vector3(30f, 1f, 30f);
            rig.HoldingPad = pad.transform;

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.SetParent(root.transform, false);
            player.transform.position = holding;
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.75f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.875f, 0f);
            controller.stepOffset = 0.4f;
            var body = player.AddComponent<DesktopPlayerController>();
            var hold = new GameObject("Hold").transform;
            hold.SetParent(player.transform, false);
            hold.localPosition = new Vector3(0f, 1.1f, 0.5f);
            body.HoldPoint = hold;
            rig.DesktopBody = player.transform;

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(player.transform, false);
            capsule.transform.localScale = new Vector3(0.56f, 0.875f, 0.56f);
            capsule.transform.localPosition = new Vector3(0f, 0.875f, 0f);
            UnityEngine.Object.DestroyImmediate(capsule.GetComponent<Collider>());
            WorldKit.Paint(capsule, WorldKit.Lit, new Color(0.25f, 0.45f, 0.62f));

            var cameraGo = new GameObject("DesktopCamera");
            cameraGo.transform.SetParent(root.transform, false);
            var camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();
            var orbit = cameraGo.AddComponent<ThirdPersonCamera>();
            orbit.Target = player.transform;
            cameraGo.AddComponent<ScreenPicker>();
            var pose = player.AddComponent<PoseSource>();
            pose.Head = cameraGo.transform;
            pose.IsVr = false;

            var hud = MetaversoHud.Create("HUD-Screen", false);
            hud.transform.SetParent(root.transform, false);

            var events = new GameObject("EventSystem");
            events.transform.SetParent(root.transform, false);
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();

            var systems = new GameObject("Systems");
            systems.transform.SetParent(root.transform, false);
            systems.AddComponent<QualityTierController>();
            var fader = systems.AddComponent<TravelFader>();
            var travel = systems.AddComponent<WorldTravel>();
            travel.Rig = rig;
            travel.Fader = fader;
            var bootstrap = systems.AddComponent<Bootstrap>();
            bootstrap.Travel = travel;
            systems.AddComponent<NetworkBootstrap>();

            var xr = CreateXrRig();
            if (xr != null)
            {
                xr.transform.SetParent(root.transform, true);
                xr.transform.position = holding;
                rig.XrRig = xr.transform;
                var head = xr.GetComponentInChildren<Camera>(true);
                rig.XrHead = head != null ? head.transform : null;
            }

            WorldKit.Save(scene, WorldPaths.BootScene);
            Debug.Log("[Metaverso] Boot creado en " + WorldPaths.BootScene);
        }

        [MenuItem("Metaverso/Mundos/Regenerar Lobby")]
        public static void BuildLobby()
        {
            const string id = "lobby";
            var scene = WorldKit.NewWorld(id, "Lobby");
            WorldKit.Ground(40f, new Color(0.52f, 0.5f, 0.47f));
            WorldKit.Sun();

            WorldKit.Spawn("default", new Vector3(0f, 0f, -6f), 0f, true);
            WorldKit.Spawn("from-circuito", new Vector3(-5f, 0f, 2.5f), 150f);
            WorldKit.Spawn("from-pabellon-a", new Vector3(0f, 0f, 4.5f), 180f);

            WorldKit.Portal("circuito", new Vector3(-7f, 0f, 6f), -40f, new Color(0.95f, 0.62f, 0.3f));
            WorldKit.Portal("pabellon-a", new Vector3(0f, 0f, 8f), 0f, new Color(0.35f, 0.75f, 1f));
            WorldKit.Portal("stand-privado", new Vector3(7f, 0f, 6f), 40f, new Color(0.7f, 0.45f, 0.9f));

            var sign = WorldKit.Box(null, "Totem", new Vector3(0f, 1.2f, -2f), new Vector3(3.2f, 2.4f, 0.2f), new Color(0.86f, 0.82f, 0.74f));
            WorldKit.Sign(sign.transform.parent, "Metaverso Sinestesia\nLobby", new Vector3(0f, 1.6f, -2.12f), 0f);

            WorldKit.AddTeleportAreas();
            WorldKit.Save(scene, WorldPaths.WorldScene(id));
        }

        [MenuItem("Metaverso/Mundos/Regenerar Pabellon A")]
        public static void BuildPabellonA()
        {
            const string id = "pabellon-a";
            var scene = WorldKit.NewWorld(id, "Pabellon A");
            WorldKit.Ground(44f, new Color(0.5f, 0.49f, 0.47f));
            WorldKit.Sun();

            WorldKit.Spawn("default", new Vector3(0f, 0f, -14f), 0f, true);
            WorldKit.Portal("lobby", new Vector3(0f, 0f, -18f), 0f, new Color(0.95f, 0.72f, 0.35f));

            var xs = new[] { -10.5f, -3.5f, 3.5f, 10.5f };
            var stands = new GameObject("Stands").transform;
            for (var i = 0; i < 8; i++)
            {
                var north = i < 4;
                var x = xs[i % 4];
                var z = north ? 7f : -3f;
                var number = i + 1;
                var root = new GameObject("Stand-" + number).transform;
                root.SetParent(stands, false);
                root.position = new Vector3(x, 0f, z);

                WorldKit.Box(root, "pad-stand-" + number, new Vector3(0f, 0.075f, 0f), new Vector3(6f, 0.15f, 3f), new Color(0.62f, 0.6f, 0.56f));
                WorldKit.Box(root, "Back", new Vector3(0f, 1.4f, 1.45f), new Vector3(6f, 2.8f, 0.1f), new Color(0.88f, 0.86f, 0.82f));
                WorldKit.Sign(root, "Stand " + number, root.position + new Vector3(0f, 2.1f, 1.38f), 0f, 0.1f);
                WorldKit.Spawn("stand-" + number, root.position + new Vector3(0f, 0f, -3f), 0f);
            }

            WorldKit.AddTeleportAreas();
            WorldKit.Save(scene, WorldPaths.WorldScene(id));
        }

        static GameObject CreateXrRig()
        {
            var builder = Type.GetType("Metaverso.EditorXR.PlayerRigBuilder, Metaverso.EditorXR");
            var create = builder?.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            if (create == null)
            {
                Debug.LogWarning("[Metaverso] Sin Metaverso.EditorXR: Boot queda solo con el rig de PC.");
                return null;
            }

            try
            {
                create.Invoke(null, null);
            }
            catch (TargetInvocationException error)
            {
                Debug.LogError("[Metaverso] El rig VR fallo; Boot queda solo con el rig de PC. " + error.InnerException);
                return null;
            }

            return GameObject.Find("XR Origin");
        }
    }
}
