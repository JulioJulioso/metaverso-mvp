using System;
using System.Text;

namespace Metaverso
{
    /// <summary>
    /// Lee ?room= y ?name= de la URL. En WebGL es Application.absoluteURL.
    /// </summary>
    public static class RoomQuery
    {
        public const string DefaultRoom = "lobby";
        public const string DefaultName = "Visitante";

        public static string FromUrl(string url, string fallback = DefaultRoom)
        {
            var value = ReadParam(url, "room");
            return string.IsNullOrEmpty(value) ? fallback : SanitizeRoom(value);
        }

        public static string DisplayNameFromUrl(string url, string fallback = DefaultName)
        {
            var value = ReadParam(url, "name");
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            value = value.Trim();
            return value.Length > 24 ? value.Substring(0, 24) : value;
        }

        public static string SanitizeRoom(string room)
        {
            if (string.IsNullOrWhiteSpace(room))
                return DefaultRoom;

            var builder = new StringBuilder(room.Length);
            foreach (var c in room.Trim().ToLowerInvariant())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_')
                    builder.Append(c);
            }

            if (builder.Length == 0)
                return DefaultRoom;
            if (builder.Length > 32)
                builder.Length = 32;
            return builder.ToString();
        }

        static string ReadParam(string url, string key)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
                return null;

            var queryStart = url.IndexOf('?');
            if (queryStart < 0 || queryStart >= url.Length - 1)
                return null;

            var query = url.Substring(queryStart + 1);
            var hash = query.IndexOf('#');
            if (hash >= 0)
                query = query.Substring(0, hash);

            var parts = query.Split('&');
            for (var i = 0; i < parts.Length; i++)
            {
                var kv = parts[i].Split(new[] { '=' }, 2);
                if (kv.Length != 2 || !string.Equals(kv[0], key, StringComparison.OrdinalIgnoreCase))
                    continue;
                return Uri.UnescapeDataString(kv[1].Replace('+', ' '));
            }

            return null;
        }
    }
}
