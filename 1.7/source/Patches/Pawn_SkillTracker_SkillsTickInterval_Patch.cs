using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.SkillsTickInterval))]
    public static class Pawn_SkillTracker_SkillsTickInterval_Patch
    {
        [HarmonyInfix(typeof(SkillRecord), nameof(SkillRecord.Interval))]
        public static bool Prefix()
        {
            return !Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableSkillDecayKey);
        }
    }
}
