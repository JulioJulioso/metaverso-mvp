using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Cuenta los marcadores del circuito. Cuando llega a cero completa el paso coins_all.
    /// </summary>
    public class CoinGate : MonoBehaviour
    {
        public int Remaining;
        public string StepId = "coins_all";
        public CircuitTracker Circuit;
        public MetaversoHud Hud;

        public void NotifyCollected()
        {
            Remaining = Mathf.Max(0, Remaining - 1);
            if (Remaining > 0)
                return;

            Circuit?.Complete(StepId);
            Hud?.ShowMessage("Todos los marcadores recogidos. Lleva la pelota a los sitios 1, 2 y 3.");
        }
    }
}
