using System;
using System.Collections.Generic;

namespace Metaverso
{
    /// <summary>
    /// Donde aparece el jugador al entrar a un mundo, en este orden:
    /// el spawn pedido (?spawn= o el portal), "from-{mundo anterior}", el marcado default, el primero.
    /// </summary>
    public static class SpawnRules
    {
        public const string ArrivalPrefix = "from-";

        public static int Choose(IReadOnlyList<string> ids, IReadOnlyList<bool> defaults, string requested, string fromWorld)
        {
            if (ids == null || ids.Count == 0)
                return -1;

            var index = IndexOf(ids, requested);
            if (index >= 0)
                return index;

            if (!string.IsNullOrEmpty(fromWorld))
            {
                index = IndexOf(ids, ArrivalPrefix + fromWorld);
                if (index >= 0)
                    return index;
            }

            if (defaults != null)
            {
                for (var i = 0; i < defaults.Count && i < ids.Count; i++)
                {
                    if (defaults[i])
                        return i;
                }
            }

            return 0;
        }

        static int IndexOf(IReadOnlyList<string> ids, string id)
        {
            if (string.IsNullOrEmpty(id))
                return -1;
            for (var i = 0; i < ids.Count; i++)
            {
                if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }
    }
}
