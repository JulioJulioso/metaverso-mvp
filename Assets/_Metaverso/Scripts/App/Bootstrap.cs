using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Metaverso
{
    /// <summary>
    /// Escena Boot: lee ?world=, ?spawn=, ?room=, ?name= y ?quality=, elige la calidad, baja el catalogo y entra al primer mundo.
    /// Sin ?world= va al default del catalogo; un mundo desconocido o privado tambien, con aviso.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        public const string EditorWorldKey = "Metaverso.EditorWorld";

        public WorldTravel Travel;

        [Tooltip("Solo editor: query de prueba, p. ej. ?world=pabellon-a&spawn=stand-3. Vacio usa la escena abierta al dar Play.")]
        public string EditorQuery = "";

        public static UrlParams Entry { get; private set; }

        IEnumerator Start()
        {
            var url = EntryUrl();
            Entry = UrlState.Read(url);
            Debug.Log($"[Metaverso] Entrada: world='{Entry.World}' spawn='{Entry.Spawn}' room='{Entry.Room}'.");
            QualityTierController.Ensure(gameObject).Detect(url);

            var init = Addressables.InitializeAsync(false);
            while (!init.IsDone)
                yield return null;
            if (init.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                Debug.LogError("[Metaverso] Addressables no inicializo: " + init.OperationException);
            init.Release();

            WorldCatalog catalog = null;
            yield return WorldCatalogLoader.Load(loaded => catalog = loaded);
            if (catalog == null || Travel == null)
            {
                MetaversoHud.SetStatus("No se pudo leer el catalogo de mundos. Recarga la pagina.");
                yield break;
            }

            Travel.SetCatalog(catalog);
            var route = catalog.Route(Entry.World);
            switch (route.Outcome)
            {
                case WorldRouteOutcome.NotFound:
                    MetaversoHud.Notify($"El mundo '{Entry.World}' no existe. Entraste a {route.Entry.DisplayName}.");
                    break;
                case WorldRouteOutcome.NeedsPassword:
                    MetaversoHud.Notify(WorldTravel.PrivateMessage(route.Requested));
                    break;
            }

            var spawn = route.Outcome == WorldRouteOutcome.Found ? Entry.Spawn : null;
            Travel.Go(route.Entry.id, spawn);
        }

        string EntryUrl()
        {
#if UNITY_EDITOR
            var query = EditorQuery;
            if (string.IsNullOrWhiteSpace(query))
            {
                var world = UnityEditor.SessionState.GetString(EditorWorldKey, "");
                query = string.IsNullOrEmpty(world) ? "" : "?world=" + world;
            }
            else if (!query.StartsWith("?"))
            {
                query = "?" + query;
            }

            var url = UrlBridge.EditorPageUrl + query.Trim();
            UrlBridge.SetEditorUrl(url);
            return url;
#else
            return UrlBridge.CurrentUrl;
#endif
        }
    }
}
