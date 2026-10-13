using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.ConsumeFuel))]
    public static class Building_GravEngine_ConsumeFuel_Patch
    {
        public static void Postfix(Building_GravEngine __instance)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableGravshipCooldownKey))
            {
                return;
            }

            __instance.cooldownCompleteTick = GenTicks.TicksGame;
        }
    }
}
