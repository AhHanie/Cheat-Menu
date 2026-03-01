using System;
using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool SendCheatMessages = true;
        public static bool ClearCachedSearchOnMenuReopen = false;
        public static HashSet<string> keysEnabledByDefault = new HashSet<string>();

        public static bool GetToggleCheatDefaultEnabled(string key)
        {
            return keysEnabledByDefault.Contains(key);
        }

        public static void SetToggleCheatDefaultEnabled(string key, bool enabled)
        {
            if (enabled)
            {
                keysEnabledByDefault.Add(key);
            }
            else
            {
                keysEnabledByDefault.Remove(key);
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SendCheatMessages, "sendCheatMessages", true);
            Scribe_Values.Look(ref ClearCachedSearchOnMenuReopen, "clearCachedSearchOnMenuReopen", true);

            Scribe_Collections.Look(ref keysEnabledByDefault, "keysEnabledByDefault", LookMode.Value);

            if (keysEnabledByDefault == null)
            {
                keysEnabledByDefault = new HashSet<string>();
            }
        }
    }
}
