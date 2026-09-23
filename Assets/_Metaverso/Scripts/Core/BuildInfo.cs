using System;
using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Version visible en el HUD. El menu de build incrementa el patch.
    /// </summary>
    public class BuildInfo : ScriptableObject
    {
        public string version = "0.1.0";
        public string builtAt = "";

        public string Label => string.IsNullOrEmpty(builtAt) ? $"v{version}" : $"v{version} | {builtAt}";

        public static string NextPatch(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "0.1.1";

            var parts = version.Trim().Split('.');
            if (parts.Length != 3
                || !int.TryParse(parts[0], out var major)
                || !int.TryParse(parts[1], out var minor)
                || !int.TryParse(parts[2], out var patch))
            {
                return "0.1.1";
            }

            return $"{major}.{minor}.{patch + 1}";
        }

        public static string StampNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
