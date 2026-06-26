using HarmonyLib;
using RimWorld;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Ability), "PreActivate")]
    public static class Ability_PreActivate_Patch
    {
        public static void Postfix(Ability __instance)
        {
            AbilityCooldownToggleUtility.RestoreCharges(__instance);
        }
    }
}
