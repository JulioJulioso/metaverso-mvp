using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metaverso
{
    [Serializable]
    public class WorldStand
    {
        public string id;
        public string name;
    }

    [Serializable]
    public class WorldEntry
    {
        public const string PublicAccess = "public";
        public const string PrivateAccess = "private";

        public string id;
        public string name;
        public string access = PublicAccess;
        public string address;
        public string thumbnail;
        public string description;
        public WorldStand[] stands = new WorldStand[0];

        public bool IsPrivate => string.Equals(access, PrivateAccess, StringComparison.OrdinalIgnoreCase);
        public string DisplayName => string.IsNullOrWhiteSpace(name) ? id : name;
    }

    public enum WorldRouteOutcome
    {
        Found,
        Default,
        NotFound,
        NeedsPassword
    }

    public readonly struct WorldRoute
    {
        public readonly WorldEntry Entry;
        public readonly WorldRouteOutcome Outcome;
        public readonly WorldEntry Requested;

        public WorldRoute(WorldEntry entry, WorldRouteOutcome outcome, WorldEntry requested)
        {
            Entry = entry;
            Outcome = outcome;
            Requested = requested;
        }
    }

    /// <summary>
    /// Catalogo de mundos (worlds.json). Se edita sin recompilar: el reproductor lo baja
    /// del sitio y usa la copia de Resources si no llega.
    /// </summary>
    public sealed class WorldCatalog
    {
        [Serializable]
        class Data
        {
            public int version;
            public string defaultWorld;
            public WorldEntry[] worlds;
        }

        readonly List<WorldEntry> _worlds;
        readonly Dictionary<string, WorldEntry> _byId;

        WorldCatalog(List<WorldEntry> worlds, WorldEntry defaultWorld, int version)
        {
            _worlds = worlds;
            _byId = new Dictionary<string, WorldEntry>(worlds.Count);
            for (var i = 0; i < worlds.Count; i++)
                _byId[worlds[i].id] = worlds[i];
            Default = defaultWorld;
            Version = version;
        }

        public IReadOnlyList<WorldEntry> Worlds => _worlds;
        public WorldEntry Default { get; }
        public int Version { get; }

        public WorldEntry Find(string id)
        {
            id = UrlState.SanitizeId(id);
            return id != null && _byId.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>
        /// Adonde entra alguien que pidio ?world=. Un mundo privado vuelve con NeedsPassword
        /// y Entry apunta al default hasta que el backend valide el password.
        /// </summary>
        public WorldRoute Route(string requestedId)
        {
            if (string.IsNullOrEmpty(requestedId))
                return new WorldRoute(Default, WorldRouteOutcome.Default, null);

            var requested = Find(requestedId);
            if (requested == null)
                return new WorldRoute(Default, WorldRouteOutcome.NotFound, null);
            if (requested.IsPrivate)
                return new WorldRoute(Default, WorldRouteOutcome.NeedsPassword, requested);
            return new WorldRoute(requested, WorldRouteOutcome.Found, requested);
        }

        /// <summary>
        /// Descarta entradas sin id valido, repetidas o sin clave Addressable. Falla si no queda
        /// ningun mundo publico para usar como default.
        /// </summary>
        public static bool TryParse(string json, out WorldCatalog catalog, out string error)
        {
            catalog = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "worlds.json vacio";
                return false;
            }

            Data data;
            try
            {
                data = JsonUtility.FromJson<Data>(json);
            }
            catch (Exception exception)
            {
                error = "worlds.json invalido: " + exception.Message;
                return false;
            }

            if (data?.worlds == null || data.worlds.Length == 0)
            {
                error = "worlds.json no trae mundos";
                return false;
            }

            var worlds = new List<WorldEntry>(data.worlds.Length);
            var seen = new HashSet<string>();
            var skipped = new List<string>();
            for (var i = 0; i < data.worlds.Length; i++)
            {
                var entry = data.worlds[i];
                var id = UrlState.SanitizeId(entry?.id);
                if (id == null || string.IsNullOrWhiteSpace(entry.address) || !seen.Add(id))
                {
                    skipped.Add(entry?.id ?? "(null)");
                    continue;
                }

                entry.id = id;
                entry.address = entry.address.Trim();
                entry.access = string.IsNullOrWhiteSpace(entry.access) ? WorldEntry.PublicAccess : entry.access.Trim().ToLowerInvariant();
                entry.stands ??= new WorldStand[0];
                worlds.Add(entry);
            }

            WorldEntry fallback = null;
            var wanted = UrlState.SanitizeId(data.defaultWorld);
            for (var i = 0; i < worlds.Count; i++)
            {
                if (worlds[i].IsPrivate)
                    continue;
                if (fallback == null)
                    fallback = worlds[i];
                if (worlds[i].id == wanted)
                {
                    fallback = worlds[i];
                    break;
                }
            }

            if (fallback == null)
            {
                error = "worlds.json no tiene un mundo publico para usar como default";
                return false;
            }

            catalog = new WorldCatalog(worlds, fallback, data.version);
            error = skipped.Count == 0 ? null : "Entradas descartadas: " + string.Join(", ", skipped);
            return true;
        }
    }
}
