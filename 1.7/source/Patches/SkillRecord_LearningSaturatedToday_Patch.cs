using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(SkillRecord), "get_LearningSaturatedToday")]
    public static class SkillRecord_LearningSaturatedToday_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableLearningSaturationKey))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
