using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(DeepResourceGrid), "SetAt")]
    public static class DeepResourceGrid_SetAt_Patch
    {
        public static bool Prefix()
        {
            if (!CompDeepDrill_TryProducePortion_Patch.DeepDrillingFlag)
            {
                return true;
            }

            return false;
        }
    }
}
