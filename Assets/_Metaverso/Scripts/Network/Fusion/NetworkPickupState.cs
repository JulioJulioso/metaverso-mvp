#if PHOTON_FUSION
using Fusion;
using UnityEngine;

namespace Metaverso.Network
{
    /// <summary>
    /// Estado compartido de la pelota. Quien la toma pide StateAuthority.
    /// </summary>
    public class NetworkPickupState : NetworkBehaviour
    {
        public PickupBall Ball;

        [Networked] public Vector3 NetPosition { get; set; }
        [Networked] public NetworkBool Held { get; set; }

        public override void FixedUpdateNetwork()
        {
            if (Ball == null)
                Ball = GetComponent<PickupBall>();
            if (Ball == null || !HasStateAuthority)
                return;
            Held = Ball.IsHeld;
            if (!Ball.IsHeld)
                NetPosition = transform.position;
        }

        public override void Render()
        {
            if (HasStateAuthority || Ball == null || Ball.IsHeld || Held)
                return;
            transform.position = Vector3.Lerp(transform.position, NetPosition, Time.deltaTime * 12f);
        }

        public void Claim()
        {
            if (Object != null && !HasStateAuthority)
                Object.RequestStateAuthority();
        }
    }
}
#endif
