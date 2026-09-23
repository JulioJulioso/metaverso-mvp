using UnityEditor;
using UnityEngine;

namespace Metaverso.EditorTools
{
    public static class ArchModelMenu
    {
        [MenuItem("Metaverso/Preparar modelo arquitectonico")]
        public static void PrepareSelection()
        {
            var roots = Selection.gameObjects;
            if (roots == null || roots.Length == 0)
            {
                EditorUtility.DisplayDialog("Metaverso", "Selecciona el modelo importado en la escena o en el proyecto.", "Ok");
                return;
            }

            var triangles = 0;
            var renderers = 0;
            var maxTexture = 0;
            for (var i = 0; i < roots.Length; i++)
                Prepare(roots[i], ref triangles, ref renderers, ref maxTexture);

            var report = QuestBudget.Evaluate(renderers, triangles, maxTexture);
            var lod = "LODGroup: no se genera solo. Exporta LODs desde Rhino o Revit si el modelo se pasa de triangulos.";
            Debug.Log($"[Metaverso] {report.Summary}. {lod}");
            EditorUtility.DisplayDialog("Presupuesto Quest", report.Summary + "\n\n" + lod, "Ok");
        }

        public static void Prepare(GameObject root, ref int triangles, ref int renderers, ref int maxTexture)
        {
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];
                var mesh = filter.sharedMesh;
                if (mesh == null)
                    continue;
                triangles += mesh.triangles.Length / 3;
                var collider = filter.GetComponent<MeshCollider>();
                if (collider == null)
                    collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = false;
                filter.gameObject.layer = ArchImportRules.ArchitectureLayer;
                GameObjectUtility.SetStaticEditorFlags(
                    filter.gameObject,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.ContributeGI |
                    StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.ReflectionProbeStatic);
            }

            var meshes = root.GetComponentsInChildren<Renderer>(true);
            renderers += meshes.Length;
            for (var i = 0; i < meshes.Length; i++)
            {
                var materials = meshes[i].sharedMaterials;
                for (var m = 0; m < materials.Length; m++)
                {
                    if (materials[m] == null || materials[m].mainTexture == null)
                        continue;
                    var size = Mathf.Max(materials[m].mainTexture.width, materials[m].mainTexture.height);
                    if (size > maxTexture)
                        maxTexture = size;
                }
            }
        }
    }
}
