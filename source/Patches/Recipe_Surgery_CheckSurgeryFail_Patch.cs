using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
    public static class Recipe_Surgery_CheckSurgeryFail_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            Game game = Current.Game;
            CheatMenuGameComponent component = game.GetComponent<CheatMenuGameComponent>();
            if (!component.IsEnabled(ToggleCheatsGeneral.SurgeryNeverFailsKey))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
