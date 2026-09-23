using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Metaverso.EditorTools
{
    public static class TestWorldBuilder
    {
        const string ScenePath = WebBuildPipeline.ScenePath;

        [MenuItem("Metaverso/Crear mundo de prueba")]
        public static void CreateOrReplace()
        {
            if (File.Exists(ScenePath))
                AssetDatabase.DeleteAsset(ScenePath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
                lit = Shader.Find("Standard");

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            Paint(ground, lit, new Color(0.45f, 0.43f, 0.4f));
            ground.AddComponent<BimLabel>().ElementName = "Losa N0";
            var groundLabel = ground.GetComponent<BimLabel>();
            groundLabel.Category = "Floor";
            groundLabel.Note = "Espesor 200 mm";

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0f, 0f);
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

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(player.transform, false);
            capsule.transform.localScale = new Vector3(0.56f, 0.875f, 0.56f);
            capsule.transform.localPosition = new Vector3(0f, 0.875f, 0f);
            UnityEngine.Object.DestroyImmediate(capsule.GetComponent<Collider>());
            Paint(capsule, lit, new Color(0.25f, 0.45f, 0.62f));

            var cameraGo = new GameObject("DesktopCamera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();
            var orbit = cameraGo.AddComponent<ThirdPersonCamera>();
            orbit.Target = player.transform;
            cameraGo.AddComponent<ScreenPicker>();
            var pose = player.AddComponent<PoseSource>();
            pose.Head = cameraGo.transform;
            pose.IsVr = false;

            AddPlatform(lit, "pad-start", new Vector3(2.5f, 0.2f, 1.5f), new Vector3(2.2f, 0.4f, 2.2f), new Color(0.42f, 0.4f, 0.38f), false);
            AddPlatform(lit, "pad-mid-low", new Vector3(5.2f, 0.35f, 2.8f), new Vector3(2f, 0.45f, 2f), new Color(0.4f, 0.39f, 0.37f), false);
            AddPlatform(lit, "pad-jump-high", new Vector3(8f, 1.45f, 3.5f), new Vector3(2.4f, 0.35f, 2.4f), new Color(0.36f, 0.35f, 0.34f), true);
            AddPlatform(lit, "pad-bridge", new Vector3(10.5f, 1.5f, 3.5f), new Vector3(2.8f, 0.3f, 1.6f), new Color(0.38f, 0.37f, 0.35f), false);
            AddPlatform(lit, "pad-final", new Vector3(13.2f, 1.55f, 3.5f), new Vector3(2.5f, 0.35f, 2.5f), new Color(0.34f, 0.33f, 0.32f), false);

            var gate = new GameObject("Coins").AddComponent<CoinGate>();
            AddCoin(lit, gate.transform, new Vector3(2.5f, 0.85f, 1.5f));
            AddCoin(lit, gate.transform, new Vector3(5.2f, 1.05f, 2.8f));
            AddCoin(lit, gate.transform, new Vector3(8f, 2.15f, 3.5f));
            AddCoin(lit, gate.transform, new Vector3(10.5f, 2.1f, 3.5f));
            AddCoin(lit, gate.transform, new Vector3(13.5f, 2.2f, 3.5f));

            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "Ball";
            ballGo.transform.position = new Vector3(-5.5f, 0.35f, 0f);
            ballGo.transform.localScale = Vector3.one * 0.64f;
            Paint(ballGo, lit, new Color(0.72f, 0.28f, 0.22f));
            var rigid = ballGo.AddComponent<Rigidbody>();
            rigid.mass = 0.4f;
            ballGo.AddComponent<PickupBall>();

            var zone1 = AddZone(lit, "Sitio de entrega 1", "ball_zone_1", new Vector3(-7.5f, 0.02f, 3f), new Color(0.12f, 0.55f, 0.62f));
            var zone2 = AddZone(lit, "Sitio de entrega 2", "ball_zone_2", new Vector3(-7.5f, 0.02f, 0f), new Color(0.15f, 0.48f, 0.58f));
            var zone3 = AddZone(lit, "Sitio de entrega 3", "ball_zone_3", new Vector3(-7.5f, 0.02f, -3f), new Color(0.18f, 0.42f, 0.52f));
            zone1.Next = zone2;
            zone1.PreviousStep = "coins_all";
            zone2.Next = zone3;
            zone2.PreviousStep = "ball_zone_1";
            zone3.PreviousStep = "ball_zone_2";

            var wallsRoot = new GameObject("Walls");
            wallsRoot.transform.position = new Vector3(-2f, 0f, -5f);
            var walls = wallsRoot.AddComponent<WallAssembly>();
            for (var i = 0; i < 5; i++)
            {
                var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "Wall-" + (i + 1);
                panel.transform.SetParent(wallsRoot.transform, false);
                panel.transform.localScale = new Vector3(0.18f, 2.8f, 2.4f);
                panel.transform.localPosition = new Vector3(i * 0.4f, 1.4f, 0f);
                Paint(panel, lit, new Color(0.82f, 0.8f, 0.74f));
                panel.AddComponent<BimLabel>().ElementName = "Muro " + (i + 1);
            }

            AddWallTimeline(walls);

            var screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "MediaScreen";
            screen.transform.position = new Vector3(4f, 1.6f, -4.5f);
            screen.transform.localScale = new Vector3(2.4f, 1.35f, 1f);
            Paint(screen, lit, new Color(0.08f, 0.08f, 0.1f));
            var box = screen.AddComponent<BoxCollider>();
            box.size = new Vector3(1f, 1f, 0.2f);
            screen.AddComponent<MediaScreen>();
            var screenLabel = screen.AddComponent<BimLabel>();
            screenLabel.ElementName = "Pantalla";
            screenLabel.Category = "Media";

            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();

            MetaversoHud.Create("HUD-Screen", false);
            var board = new GameObject("Circuit");
            board.AddComponent<CircuitBoard>();
            board.AddComponent<NetworkBootstrap>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            var rigBuilder = Type.GetType("Metaverso.EditorXR.PlayerRigBuilder, Metaverso.EditorXR");
            try
            {
                rigBuilder?.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
            catch (TargetInvocationException error)
            {
                Debug.LogError("[Metaverso] El rig VR fallo; la escena de PC quedo guardada. " + error.InnerException);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Metaverso] Escena de circuito creada en " + ScenePath);
        }

        static GameObject AddPlatform(Shader shader, string name, Vector3 position, Vector3 size, Color color, bool high)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.tag = "Untagged";
            go.transform.position = position;
            go.transform.localScale = size;
            Paint(go, shader, color);
            if (high)
            {
                var marker = new GameObject("JumpVolume");
                marker.transform.SetParent(go.transform, false);
                marker.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                var volume = marker.AddComponent<BoxCollider>();
                volume.isTrigger = true;
                volume.size = new Vector3(0.8f, 0.5f, 0.8f);
                marker.AddComponent<JumpFlag>();
            }
            return high ? go.transform.Find("JumpVolume").gameObject : go;
        }

        static void AddCoin(Shader shader, Transform parent, Vector3 position)
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.SetParent(parent, true);
            coin.transform.position = position;
            coin.transform.localScale = new Vector3(0.35f, 0.06f, 0.35f);
            var collider = coin.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            Paint(coin, shader, new Color(0.86f, 0.72f, 0.28f));
            coin.AddComponent<CoinMarker>();
        }

        static DeliveryZone AddZone(Shader shader, string label, string step, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = step;
            go.transform.position = position;
            go.transform.localScale = new Vector3(2.7f, 0.02f, 2.7f);
            Paint(go, shader, color);
            var zone = go.AddComponent<DeliveryZone>();
            zone.Label = label;
            zone.StepId = step;
            zone.Radius = 1.35f;
            return zone;
        }

        static void AddWallTimeline(WallAssembly walls)
        {
            var dir = "Assets/_Metaverso/Scenes";
            Directory.CreateDirectory(dir);
            var assetPath = dir + "/WallRise.playable";
            if (AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, assetPath);

            var signal = ScriptableObject.CreateInstance<SignalAsset>();
            signal.name = "RiseWalls";
            AssetDatabase.AddObjectToAsset(signal, timeline);
            var track = timeline.CreateTrack<SignalTrack>(null, "Muros");
            var emitter = track.CreateMarker<SignalEmitter>(0d);
            emitter.asset = signal;

            var director = walls.gameObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = false;
            walls.Director = director;

            var receiver = walls.gameObject.AddComponent<SignalReceiver>();
            var reaction = new UnityEngine.Events.UnityEvent();
            UnityEventTools.AddPersistentListener(reaction, walls.Rise);
            receiver.AddReaction(signal, reaction);
            EditorUtility.SetDirty(timeline);
        }

        static void Paint(GameObject go, Shader shader, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null || shader == null)
                return;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else
                material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
