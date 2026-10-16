using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    // Used instead of GenRecipe_PostProcessProduct_Patch when Vanilla Skills Expanded is active: its transpiler
    // replaces the GenerateQualityCreatedByPawn call, so an infix on that call finds nothing to wrap.
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class GenRecipe_PostProcessProduct_VseCompat_Patch
    {
        public const string VsePackageId = "vanillaexpanded.skills";

        [HarmonyPrepare]
        public static bool Prepare()
        {
            return ModsConfig.IsActive(VsePackageId);
        }

        public static void Postfix(Thing __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.AlwaysCraftLegendariesKey))
            {
                return;
            }

            __result.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Colony);
        }
    }
}
