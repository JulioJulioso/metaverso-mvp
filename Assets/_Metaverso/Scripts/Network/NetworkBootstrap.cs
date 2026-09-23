using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Arranca en local. Photon Fusion sustituye la sesion cuando el define PHOTON_FUSION esta activo.
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (NetworkServices.Session != null)
                return;

            var room = RoomQuery.FromUrl(Application.absoluteURL);
            NetworkServices.Session = new OfflineSession(room);
            Debug.Log($"[Metaverso] Sala '{room}' en local. Multiplayer: importa Photon Fusion y usa Metaverso > Red > Activar Photon Fusion.");
        }
    }
}
