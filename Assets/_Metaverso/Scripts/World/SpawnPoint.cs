using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metaverso
{
    /// <summary>
    /// Punto de entrada a un mundo. Id "from-lobby" recibe a quien llega por el portal del lobby;
    /// un id como "stand-3" sirve para links directos (?spawn=stand-3). Mira hacia +Z.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        public string Id = "default";
        public bool IsDefault;

        public static SpawnPoint Choose(Scene scene, string requested, string fromWorld)
        {
            var points = new List<SpawnPoint>();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
                points.AddRange(roots[i].GetComponentsInChildren<SpawnPoint>());

            var ids = new string[points.Count];
            var defaults = new bool[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                ids[i] = points[i].Id;
                defaults[i] = points[i].IsDefault;
            }

            var index = SpawnRules.Choose(ids, defaults, requested, fromWorld);
            return index >= 0 ? points[index] : null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = IsDefault ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.3f, 0.7f, 1f);
            var position = transform.position;
            Gizmos.DrawWireSphere(position + Vector3.up * 0.1f, 0.3f);
            Gizmos.DrawLine(position + Vector3.up * 0.1f, position + Vector3.up * 0.1f + transform.forward * 0.8f);
        }
    }
}
