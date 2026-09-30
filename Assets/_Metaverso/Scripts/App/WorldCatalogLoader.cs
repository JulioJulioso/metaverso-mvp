using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Metaverso
{
    /// <summary>
    /// Baja worlds.json del sitio (sin cache, para cambios en vivo durante la feria).
    /// Si no llega o no parsea, usa la copia en Resources/worlds.json del build.
    /// </summary>
    public static class WorldCatalogLoader
    {
        public const int TimeoutSeconds = 6;

        public static IEnumerator Load(Action<WorldCatalog> done)
        {
            var url = WorldContent.CatalogUrl;
            if (!string.IsNullOrEmpty(url))
            {
                var bust = UrlState.WithParam(url, "t", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                using (var request = UnityWebRequest.Get(bust))
                {
                    request.timeout = TimeoutSeconds;
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        if (TryParse(request.downloadHandler.text, "sitio", out var remote))
                        {
                            done(remote);
                            yield break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Metaverso] No baje {url}: {request.error}. Uso el catalogo del build.");
                    }
                }
            }

            var embedded = Resources.Load<TextAsset>(WorldContent.CatalogResource);
            if (embedded != null && TryParse(embedded.text, "build", out var local))
            {
                done(local);
                yield break;
            }

            Debug.LogError("[Metaverso] No hay catalogo de mundos valido. Revisa Assets/_Metaverso/Resources/worlds.json.");
            done(null);
        }

        static bool TryParse(string json, string source, out WorldCatalog catalog)
        {
            if (!WorldCatalog.TryParse(json, out catalog, out var error))
            {
                Debug.LogWarning($"[Metaverso] Catalogo del {source} descartado: {error}");
                return false;
            }

            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"[Metaverso] Catalogo del {source}: {error}");
            Debug.Log($"[Metaverso] Catalogo del {source} v{catalog.Version}: {catalog.Worlds.Count} mundos, default '{catalog.Default.id}'.");
            return true;
        }
    }
}
