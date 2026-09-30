using System;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metaverso.EditorTools
{
    /// <summary>Piezas de blockout para generar mundos: suelo, sol, spawns, portales, carteles.</summary>
    public static class WorldKit
    {
        public static Shader Lit
        {
            get
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                return shader != null ? shader : Shader.Find("Standard");
            }
        }

        public static Shader Unlit
        {
            get
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                return shader != null ? shader : Shader.Find("Unlit/Color");
            }
        }

        public static Scene NewWorld(string worldId, string displayName)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("World");
            var descriptor = root.AddComponent<WorldDescriptor>();
            descriptor.WorldId = worldId;
            descriptor.DisplayName = displayName;
            return scene;
        }

        public static GameObject Ground(float sizeMeters, Color color)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(sizeMeters / 10f, 1f, sizeMeters / 10f);
            Paint(ground, Lit, color);
            return ground;
        }

        public static Light Sun()
        {
            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -30f, 0f);
            return light;
        }

        public static SpawnPoint Spawn(string id, Vector3 position, float yaw, bool isDefault = false)
        {
            var go = new GameObject("Spawn-" + id);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var spawn = go.AddComponent<SpawnPoint>();
            spawn.Id = id;
            spawn.IsDefault = isDefault;
            return spawn;
        }

        /// <summary>Marco de 2.4 x 2.8 m con superficie sin collider: se cruza caminando.</summary>
        public static Portal Portal(string destination, Vector3 position, float yaw, Color tint, string destinationSpawn = null)
        {
            var root = new GameObject("Portal-" + destination);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var frame = new Color(0.22f, 0.21f, 0.2f);
            Box(root.transform, "PostL", new Vector3(-1.2f, 1.4f, 0f), new Vector3(0.2f, 2.8f, 0.3f), frame);
            Box(root.transform, "PostR", new Vector3(1.2f, 1.4f, 0f), new Vector3(0.2f, 2.8f, 0.3f), frame);
            Box(root.transform, "Lintel", new Vector3(0f, 2.9f, 0f), new Vector3(2.6f, 0.2f, 0.3f), frame);

            var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Surface";
            surface.transform.SetParent(root.transform, false);
            surface.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            surface.transform.localScale = new Vector3(2.2f, 2.8f, 0.04f);
            UnityEngine.Object.DestroyImmediate(surface.GetComponent<Collider>());
            Paint(surface, Unlit, tint);

            var portal = root.AddComponent<Portal>();
            portal.DestinationWorld = destination;
            portal.DestinationSpawn = destinationSpawn;
            portal.Surface = surface.GetComponent<Renderer>();
            portal.Tint = tint;
            return portal;
        }

        public static GameObject Box(Transform parent, string name, Vector3 localPosition, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            Paint(go, Lit, color);
            return go;
        }

        /// <summary>Cartel fijo. TextMesh se lee desde su lado -Z: con yaw 0 se lee mirando hacia +Z.</summary>
        public static TextMesh Sign(Transform parent, string text, Vector3 position, float yaw, float size = 0.08f)
        {
            var go = new GameObject("Sign");
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var mesh = go.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mesh.font = font;
            mesh.text = text;
            mesh.fontSize = 64;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.12f, 0.1f, 0.08f);
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return mesh;
        }

        public static void Paint(GameObject go, Shader shader, Color color)
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

        /// <summary>TeleportationArea en "Ground" y "pad-*". Vive en EditorXR, que puede no compilar sin WebXR.</summary>
        public static void AddTeleportAreas()
        {
            var builder = Type.GetType("Metaverso.EditorXR.PlayerRigBuilder, Metaverso.EditorXR");
            var method = builder?.GetMethod("AddTeleportAreas", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogWarning("[Metaverso] Sin Metaverso.EditorXR: el mundo queda sin areas de teletransporte VR.");
                return;
            }

            method.Invoke(null, null);
        }

        public static void Save(Scene scene, string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? WorldPaths.WorldsFolder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
