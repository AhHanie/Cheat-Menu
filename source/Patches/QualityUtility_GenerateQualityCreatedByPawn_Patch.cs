using HarmonyLib;
using RimWorld;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn", new[] { typeof(int), typeof(bool) })]
    public static class QualityUtility_GenerateQualityCreatedByPawn_Patch
    {
        public static bool Prefix(ref QualityCategory __result)
        {
            if (!GenRecipe_PostProcessProduct_Patch.PostProcessProductFlag)
            {
                return true;
            }

            __result = QualityCategory.Legendary;
            return false;
        }
    }
}

