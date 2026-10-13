using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "get_TotalBandwidth")]
    public static class Pawn_MechanitorTracker_TotalBandwidth_Patch
    {
        private const int InfiniteMechanitorBandwidth = 99;

        public static void Postfix(ref int __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.InfiniteMechanitorBandwidthKey))
            {
                return;
            }

            __result = InfiniteMechanitorBandwidth;
        }
    }
}
