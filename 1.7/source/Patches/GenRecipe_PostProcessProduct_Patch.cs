using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class GenRecipe_PostProcessProduct_Patch
    {
        [HarmonyInfix(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
        public static QualityCategory Postfix(QualityCategory __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.AlwaysCraftLegendariesKey))
            {
                return __result;
            }

            return QualityCategory.Legendary;
        }
    }
}
