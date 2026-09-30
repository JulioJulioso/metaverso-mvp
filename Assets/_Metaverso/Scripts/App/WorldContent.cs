using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.Scripting;

namespace Metaverso
{
    /// <summary>
    /// Donde viven los bundles de los mundos y el catalogo de mundos, relativos a la pagina:
    /// .../desktop/worlds/ y .../worlds.json. El perfil de Addressables usa
    /// {Metaverso.WorldContent.RemoteBase} como Remote.LoadPath.
    /// </summary>
    [Preserve]
    public static class WorldContent
    {
        public const string RemoteBaseProperty = "Metaverso.WorldContent.RemoteBase";
        public const string BundleFolder = "worlds";
        public const string CatalogFile = "worlds.json";
        public const string CatalogResource = "worlds";

        [Preserve]
        public static string RemoteBase
        {
            get
            {
                var page = Application.absoluteURL;
                if (IsHttp(page))
                    return UrlState.DirectoryOf(page) + BundleFolder;
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ServerData", "WebGL")).Replace('\\', '/');
            }
        }

        /// <summary>worlds.json va en la raiz del sitio, compartido por desktop/ y quest/.</summary>
        public static string CatalogUrl
        {
            get
            {
                var page = Application.absoluteURL;
                return IsHttp(page) ? UrlState.DirectoryOf(page) + "../" + CatalogFile : null;
            }
        }

        static bool IsHttp(string url)
        {
            return !string.IsNullOrEmpty(url)
                && (url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase));
        }

        // Por reflexion Addressables tambien lo encuentra, pero IL2CPP podria quitar la propiedad.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            AddressablesRuntimeProperties.SetPropertyValue(RemoteBaseProperty, RemoteBase);
        }
    }
}
