using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Cheat_Menu
{
    public static class ToggleCheatRegistry
    {
        private static readonly Dictionary<string, ToggleCheatMetadata> cheatsByKey =
            new Dictionary<string, ToggleCheatMetadata>(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyList<ToggleCheatMetadata> AllCheats
        {
            get
            {
                return cheatsByKey.Values.ToList();
            }
        }
        public static bool Register(string key, ToggleCheatMetadata metadata, bool replaceExisting = false)
        {
            if (key.NullOrEmpty())
            {
                return false;
            }

            if (metadata == null)
            {
                return false;
            }

            if (cheatsByKey.ContainsKey(key) && !replaceExisting)
            {
                UserLogger.Warning("Duplicate toggle cheat key '" + key + "' ignored.");
                return false;
            }

            metadata.Key = key;
            cheatsByKey[key] = metadata;

            return true;
        }

        public static ToggleCheatMetadata Get(string key)
        {
            TryGet(key, out ToggleCheatMetadata metadata);
            return metadata;
        }

        public static bool TryGet(string key, out ToggleCheatMetadata metadata)
        {
            return cheatsByKey.TryGetValue(key, out metadata);
        }
    }
}
