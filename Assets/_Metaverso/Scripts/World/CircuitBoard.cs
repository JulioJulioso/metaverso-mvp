using System.Collections.Generic;
using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Dueno del circuito. En Awake reparte la misma instancia a monedas, muros, zonas y HUD.
    /// Los mensajes de cualquier HUD llegan a todos (ver MetaversoHud.ShowMessage).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CircuitBoard : MonoBehaviour
    {
        public readonly CircuitTracker Tracker = new CircuitTracker();

        static readonly KeyValuePair<string, string>[] Steps =
        {
            new KeyValuePair<string, string>("walls_rise", "Levantar muros (acercate y usa el boton)"),
            new KeyValuePair<string, string>("walls_explode", "Vision explotada de muros"),
            new KeyValuePair<string, string>("coins_all", "Recoger todos los marcadores"),
            new KeyValuePair<string, string>("jump_high", "Llegar a la plataforma alta"),
            new KeyValuePair<string, string>("ball_zone_1", "Llevar la pelota al sitio 1"),
            new KeyValuePair<string, string>("ball_zone_2", "Llevar la pelota al sitio 2"),
            new KeyValuePair<string, string>("ball_zone_3", "Llevar la pelota al sitio 3")
        };

        void Awake()
        {
            Tracker.SetSteps(Steps);

            var huds = FindObjectsByType<MetaversoHud>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var hud = huds.Length > 0 ? huds[0] : null;
            var walls = FindAnyObjectByType<WallAssembly>();
            var gate = FindAnyObjectByType<CoinGate>();
            var ball = FindAnyObjectByType<PickupBall>();

            for (var i = 0; i < huds.Length; i++)
                huds[i].Bind(Tracker, walls);

            var media = FindObjectsByType<MediaScreen>(FindObjectsSortMode.None);
            for (var i = 0; i < media.Length; i++)
                media[i].Hud = hud;

            var pickers = FindObjectsByType<ScreenPicker>(FindObjectsSortMode.None);
            for (var i = 0; i < pickers.Length; i++)
                pickers[i].Hud = hud;

            if (walls != null)
            {
                walls.Circuit = Tracker;
                walls.Hud = hud;
            }

            var coins = FindObjectsByType<CoinMarker>(FindObjectsSortMode.None);
            if (gate != null)
            {
                gate.Circuit = Tracker;
                gate.Hud = hud;
                gate.Remaining = coins.Length;
            }

            for (var i = 0; i < coins.Length; i++)
            {
                coins[i].Gate = gate;
                coins[i].Hud = hud;
            }

            var zones = FindObjectsByType<DeliveryZone>(FindObjectsSortMode.None);
            for (var i = 0; i < zones.Length; i++)
            {
                zones[i].Circuit = Tracker;
                zones[i].Hud = hud;
                zones[i].Ball = ball;
            }

            var jumps = FindObjectsByType<JumpFlag>(FindObjectsSortMode.None);
            for (var i = 0; i < jumps.Length; i++)
                jumps[i].Circuit = Tracker;

            if (ball != null)
                ball.Hud = hud;
        }
    }
}
