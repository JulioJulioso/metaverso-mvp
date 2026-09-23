using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Cabeza y manos que el avatar de red copia. Hay una en el rig de PC y otra en el de VR.
    /// </summary>
    public class PoseSource : MonoBehaviour
    {
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public bool IsVr;

        public static PoseSource Active()
        {
            var all = FindObjectsByType<PoseSource>(FindObjectsSortMode.None);
            for (var i = 0; i < all.Length; i++)
            {
                var source = all[i];
                if (!source.isActiveAndEnabled || source.Head == null)
                    continue;
                if (source.Head.gameObject.activeInHierarchy)
                    return source;
            }

            return null;
        }
    }
}
