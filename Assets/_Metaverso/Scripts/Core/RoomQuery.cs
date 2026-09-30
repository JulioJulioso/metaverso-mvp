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
            var value = UrlState.ReadParam(url, UrlState.RoomKey);
            return string.IsNullOrEmpty(value) ? fallback : SanitizeRoom(value);
        }

        public static string DisplayNameFromUrl(string url, string fallback = DefaultName)
        {
            var value = UrlState.ReadParam(url, UrlState.NameKey);
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
    }
}
