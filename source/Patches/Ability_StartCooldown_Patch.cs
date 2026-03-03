using HarmonyLib;
using Verse;
using RimWorld;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Ability), nameof(Ability.StartCooldown))]
    public static class Ability_StartCooldown_Patch
    {
        public static bool Prefix(Ability __instance)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableAbilityCooldownKey))
            {
                return true;
            }

            if (__instance.pawn == null || !__instance.pawn.IsColonist)
            {
                return true; 
            }

            return false;
        }
    }
}
