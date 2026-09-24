using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Installs/removes the optional SkyManager.SkyManagerUpdate render postfix on demand, so maps
    /// left at the Normal preset never pay for a per-frame Harmony dispatch. Patch/unpatch only
    /// happens at state transitions (menu selection, save load/new game, map removal, game dispose).
    /// </summary>
    public static class VisualTimeOfDayPatchController
    {
        private const string HarmonyId = "sk.cheatmenu.visualtimeofday";

        private static readonly Harmony HarmonyInstance = new Harmony(HarmonyId);

        private static MethodInfo originalMethod;
        private static MethodInfo postfixMethod;
        private static bool isPatched;

        public static void Refresh()
        {
            bool shouldBePatched = AnyMapHasActivePreset();
            if (shouldBePatched == isPatched)
            {
                return;
            }

            if (shouldBePatched)
            {
                HarmonyInstance.Patch(OriginalMethod(), postfix: new HarmonyMethod(PostfixMethod()));
            }
            else
            {
                HarmonyInstance.Unpatch(OriginalMethod(), PostfixMethod());
            }

            isPatched = shouldBePatched;
        }

        public static void Clear()
        {
            if (!isPatched)
            {
                return;
            }

            HarmonyInstance.Unpatch(OriginalMethod(), PostfixMethod());
            isPatched = false;
        }

        private static bool AnyMapHasActivePreset()
        {
            List<Map> maps = Find.Maps;
            if (maps == null)
            {
                return false;
            }

            for (int i = 0; i < maps.Count; i++)
            {
                CheatMenuMapComponent component = maps[i].GetComponent<CheatMenuMapComponent>();
                if (component != null && component.VisualTimeOfDayPreset != VisualTimeOfDay.Normal)
                {
                    return true;
                }
            }

            return false;
        }

        private static MethodInfo OriginalMethod()
        {
            if (originalMethod == null)
            {
                originalMethod = AccessTools.Method(typeof(SkyManager), nameof(SkyManager.SkyManagerUpdate));
            }

            return originalMethod;
        }

        private static MethodInfo PostfixMethod()
        {
            if (postfixMethod == null)
            {
                postfixMethod = AccessTools.Method(typeof(SkyManager_SkyManagerUpdate_Patch), nameof(SkyManager_SkyManagerUpdate_Patch.Postfix));
            }

            return postfixMethod;
        }
    }
}
