using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Arranca en local. Photon Fusion sustituye la sesion cuando el define PHOTON_FUSION esta activo.
    /// Vive en el rig de Boot: la sala sigue al mundo (?room= la fuerza para reuniones privadas).
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (NetworkServices.Session != null)
                return;

            var room = RoomQuery.FromUrl(UrlBridge.CurrentUrl);
            NetworkServices.Session = new OfflineSession(room);
            Debug.Log($"[Metaverso] Sala '{room}' en local. Multiplayer: importa Photon Fusion y usa Metaverso > Red > Activar Photon Fusion.");
        }

        void OnEnable()
        {
            WorldTravel.Arrived += OnArrived;
        }

        void OnDisable()
        {
            WorldTravel.Arrived -= OnArrived;
        }

        void OnArrived(WorldEntry world)
        {
            if (NetworkServices.IsOnline)
                return;
            var room = UrlState.SessionName(Bootstrap.Entry, world.id);
            if (NetworkServices.Session != null && NetworkServices.Session.RoomName == room)
                return;
            NetworkServices.Session = new OfflineSession(room);
            Debug.Log($"[Metaverso] Sala '{room}' en local para {world.id}.");
        }
    }
}
