using HarmonyLib;
using RimWorld;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Ability), nameof(Ability.StartCooldown))]
    public static class Ability_StartCooldown_Patch
    {
        public static bool Prefix(Ability __instance)
        {
            return !AbilityCooldownToggleUtility.ShouldDisableCooldown(__instance);
        }
    }
}
