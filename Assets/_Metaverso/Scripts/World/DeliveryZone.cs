using UnityEngine;

namespace Metaverso
{
    public class DeliveryZone : MonoBehaviour
    {
        public string StepId;
        public string Label;
        public float Radius = 1.35f;
        public PickupBall Ball;
        public CircuitTracker Circuit;
        public MetaversoHud Hud;
        public DeliveryZone Next;

        bool _done;

        void Update()
        {
            if (_done || Ball == null || Ball.IsHeld || !Ball.Grounded || Circuit == null)
                return;
            if (!string.IsNullOrEmpty(PreviousStep) && !Circuit.IsDone(PreviousStep))
                return;

            var flat = Ball.transform.position - transform.position;
            flat.y = 0f;
            if (flat.magnitude > Radius)
                return;

            _done = true;
            Circuit.Complete(StepId);
            var next = Next != null ? $" Siguiente: {Next.Label}" : " Circuito de entregas completo.";
            Hud?.ShowMessage($"{Label} completado.{next}");
        }

        public string PreviousStep;
    }
}
