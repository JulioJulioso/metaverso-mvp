using System;
using System.Collections.Generic;
using System.Text;

namespace Metaverso
{
    public readonly struct UrlParams
    {
        public readonly string World;
        public readonly string Room;
        public readonly string Name;
        public readonly string Spawn;

        public UrlParams(string world, string room, string name, string spawn)
        {
            World = world;
            Room = room;
            Name = name;
            Spawn = spawn;
        }
    }

    /// <summary>
    /// Lee y reescribe la query de la URL: ?world=, ?room=, ?name=, ?spawn=.
    /// El password nunca va en la URL.
    /// </summary>
    public static class UrlState
    {
        public const string WorldKey = "world";
        public const string RoomKey = "room";
        public const string NameKey = "name";
        public const string SpawnKey = "spawn";
        public const int MaxIdLength = 32;

        static readonly string[] VariantFolders = { "desktop", "quest" };

        public static UrlParams Read(string url)
        {
            var room = ReadParam(url, RoomKey);
            return new UrlParams(
                SanitizeId(ReadParam(url, WorldKey)),
                string.IsNullOrWhiteSpace(room) ? null : RoomQuery.SanitizeRoom(room),
                RoomQuery.DisplayNameFromUrl(url, null),
                SanitizeId(ReadParam(url, SpawnKey)));
        }

        /// <summary>Minusculas, a-z 0-9 - _, hasta 32. Null si no queda nada.</summary>
        public static string SanitizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var builder = new StringBuilder(value.Length);
            foreach (var c in value.Trim().ToLowerInvariant())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_')
                    builder.Append(c);
            }

            if (builder.Length == 0)
                return null;
            if (builder.Length > MaxIdLength)
                builder.Length = MaxIdLength;
            return builder.ToString();
        }

        /// <summary>La sala de red: ?room= fuerza una sala privada; si no, la del mundo.</summary>
        public static string SessionName(UrlParams url, string worldId)
        {
            if (!string.IsNullOrEmpty(url.Room))
                return url.Room;
            return string.IsNullOrEmpty(worldId) ? RoomQuery.DefaultRoom : worldId;
        }

        public static string ReadParam(string url, string key)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
                return null;

            Split(url, out _, out var query, out _);
            if (string.IsNullOrEmpty(query))
                return null;

            var parts = query.Split('&');
            for (var i = 0; i < parts.Length; i++)
            {
                var kv = parts[i].Split(new[] { '=' }, 2);
                if (kv.Length != 2 || !string.Equals(kv[0], key, StringComparison.OrdinalIgnoreCase))
                    continue;
                return Decode(kv[1]);
            }

            return null;
        }

        /// <summary>
        /// Devuelve la URL con el parametro reemplazado. value null o vacio lo quita.
        /// Mantiene el orden del resto y el hash.
        /// </summary>
        public static string WithParam(string url, string key, string value)
        {
            Split(url ?? "", out var path, out var query, out var hash);
            var pairs = new List<string>();
            var replaced = false;
            if (!string.IsNullOrEmpty(query))
            {
                var parts = query.Split('&');
                for (var i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Length == 0)
                        continue;
                    var name = parts[i].Split(new[] { '=' }, 2)[0];
                    if (!string.Equals(name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        pairs.Add(parts[i]);
                        continue;
                    }

                    if (!replaced && !string.IsNullOrEmpty(value))
                        pairs.Add(key + "=" + Uri.EscapeDataString(value));
                    replaced = true;
                }
            }

            if (!replaced && !string.IsNullOrEmpty(value))
                pairs.Add(key + "=" + Uri.EscapeDataString(value));

            var search = pairs.Count == 0 ? "" : "?" + string.Join("&", pairs);
            return path + search + hash;
        }

        /// <summary>"?a=b" o "" (sin hash). Es lo que recibe history.replaceState.</summary>
        public static string SearchOf(string url)
        {
            Split(url ?? "", out _, out var query, out _);
            return string.IsNullOrEmpty(query) ? "" : "?" + query;
        }

        /// <summary>
        /// Link para compartir un mundo. Apunta a la raiz del sitio (no a desktop/ ni quest/)
        /// para que cada visor tome su build. Conserva ?room=; quita ?name= y ?spawn=.
        /// </summary>
        public static string ShareUrl(string url, string worldId)
        {
            Split(url ?? "", out var path, out _, out _);
            path = SiteRoot(path);
            var share = WithParam(path, WorldKey, SanitizeId(worldId));
            var room = ReadParam(url, RoomKey);
            if (!string.IsNullOrWhiteSpace(room))
                share = WithParam(share, RoomKey, RoomQuery.SanitizeRoom(room));
            return share;
        }

        /// <summary>Carpeta de la pagina, con "/" final: https://x/app/desktop/index.html -> https://x/app/desktop/</summary>
        public static string DirectoryOf(string url)
        {
            Split(url ?? "", out var path, out _, out _);
            var slash = path.LastIndexOf('/');
            var scheme = path.IndexOf("://", StringComparison.Ordinal);
            if (slash < 0 || (scheme >= 0 && slash < scheme + 3))
                return path.Length == 0 ? "" : path + "/";
            return path.Substring(0, slash + 1);
        }

        static string SiteRoot(string path)
        {
            var directory = DirectoryOf(path);
            for (var i = 0; i < VariantFolders.Length; i++)
            {
                var suffix = "/" + VariantFolders[i] + "/";
                if (directory.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return directory.Substring(0, directory.Length - suffix.Length + 1);
            }

            return directory;
        }

        static void Split(string url, out string path, out string query, out string hash)
        {
            hash = "";
            var hashAt = url.IndexOf('#');
            if (hashAt >= 0)
            {
                hash = url.Substring(hashAt);
                url = url.Substring(0, hashAt);
            }

            var queryAt = url.IndexOf('?');
            if (queryAt < 0)
            {
                path = url;
                query = "";
                return;
            }

            path = url.Substring(0, queryAt);
            query = url.Substring(queryAt + 1);
        }

        static string Decode(string value)
        {
            try
            {
                return Uri.UnescapeDataString(value.Replace('+', ' '));
            }
            catch (UriFormatException)
            {
                return value;
            }
        }
    }
}
