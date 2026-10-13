using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CompDeepDrill), "TryProducePortion")]
    public static class CompDeepDrill_TryProducePortion_Patch
    {
        [HarmonyInfix(typeof(DeepResourceGrid), nameof(DeepResourceGrid.SetAt))]
        public static bool Prefix()
        {
            return !Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.InfiniteDeepDrillingKey);
        }
    }
}
