#if PHOTON_FUSION
using Fusion;
using UnityEngine;

namespace Metaverso.Network
{
    /// <summary>
    /// En VR replica cabeza y manos. En PC replica el cuerpo.
    /// El jugador local no ve su propia copia: ya tiene el rig.
    /// </summary>
    public class NetworkAvatar : NetworkBehaviour
    {
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public Renderer[] LocalHidden;

        [Networked] public NetworkBool IsVr { get; set; }
        [Networked] public Vector3 HeadPosition { get; set; }
        [Networked] public Quaternion HeadRotation { get; set; }
        [Networked] public Vector3 LeftPosition { get; set; }
        [Networked] public Quaternion LeftRotation { get; set; }
        [Networked] public Vector3 RightPosition { get; set; }
        [Networked] public Quaternion RightRotation { get; set; }
        [Networked] public NetworkString<_32> DisplayName { get; set; }

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;
            DisplayName = RoomQuery.DisplayNameFromUrl(Application.absoluteURL);
            if (LocalHidden == null)
                return;
            for (var i = 0; i < LocalHidden.Length; i++)
            {
                if (LocalHidden[i] != null)
                    LocalHidden[i].enabled = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;
            var source = PoseSource.Active();
            if (source == null || source.Head == null)
                return;
            IsVr = source.IsVr;
            HeadPosition = source.Head.position;
            HeadRotation = source.Head.rotation;
            if (source.LeftHand != null)
            {
                LeftPosition = source.LeftHand.position;
                LeftRotation = source.LeftHand.rotation;
            }
            if (source.RightHand != null)
            {
                RightPosition = source.RightHand.position;
                RightRotation = source.RightHand.rotation;
            }
        }

        public override void Render()
        {
            if (HasStateAuthority)
                return;
            if (Head != null)
            {
                Head.SetPositionAndRotation(HeadPosition, HeadRotation);
            }
            if (LeftHand != null)
                LeftHand.SetPositionAndRotation(LeftPosition, LeftRotation);
            if (RightHand != null)
                RightHand.SetPositionAndRotation(RightPosition, RightRotation);
        }
    }
}
#endif
