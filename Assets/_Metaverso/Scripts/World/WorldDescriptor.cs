using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Raiz de un mundo. El id debe coincidir con worlds.json y con el nombre de la escena
    /// en Scenes/Worlds (la clave Addressable es "world/{id}").
    /// Un mundo trae contenido, spawns y portales; el jugador, las camaras y el HUD vienen de Boot.
    /// </summary>
    public class WorldDescriptor : MonoBehaviour
    {
        public const string AddressPrefix = "world/";

        public string WorldId;
        public string DisplayName;

        public static string AddressFor(string worldId) => AddressPrefix + worldId;
    }
}
