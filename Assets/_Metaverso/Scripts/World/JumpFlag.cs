using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Trigger sobre la plataforma alta. El jugador lo toca y se completa el paso de salto.
    /// </summary>
    public class JumpFlag : MonoBehaviour
    {
        public string StepId = "jump_high";
        public CircuitTracker Circuit;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            Circuit?.Complete(StepId);
        }
    }
}
