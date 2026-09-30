using System.IO;

namespace Metaverso.EditorTools
{
    /// <summary>
    /// Boot es la unica escena del build. Cada mundo es Scenes/Worlds/{id}.unity y se publica
    /// como Addressable "world/{id}".
    /// </summary>
    public static class WorldPaths
    {
        public const string ScenesFolder = "Assets/_Metaverso/Scenes";
        public const string BootScene = ScenesFolder + "/Boot.unity";
        public const string WorldsFolder = ScenesFolder + "/Worlds";
        public const string LegacyCircuitoScene = ScenesFolder + "/Circuito.unity";
        public const string CatalogAsset = "Assets/_Metaverso/Resources/worlds.json";

        public static string WorldScene(string worldId) => WorldsFolder + "/" + worldId + ".unity";

        public static string WorldIdOf(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath) || !scenePath.StartsWith(WorldsFolder + "/"))
                return null;
            return Path.GetFileNameWithoutExtension(scenePath);
        }
    }
}
