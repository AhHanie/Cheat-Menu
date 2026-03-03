using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public static class VREAndroidsToggleCheats
    {
        public const string PackageId = "vanillaracesexpanded.android";
        public const string ResetNeutroLossKey = "CheatMenu.Toggle.VREAndroids.ResetNeutroLoss";

        public static void Register()
        {
            ToggleCheatRegistry.Register(
                ResetNeutroLossKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.VREAndroids.ResetNeutroLoss.Label",
                    "CheatMenu.ToggleCheat.VREAndroids.ResetNeutroLoss.Description",
                    "CheatMenu.Category.VREAndroids"));
        }

        public static void Apply(CheatMenuGameComponent gameComponent, List<Pawn> pawns)
        {
            if (!gameComponent.IsEnabled(ResetNeutroLossKey))
            {
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!VREAndroidsReflection.IsAndroid(pawn))
                {
                    continue;
                }

                Hediff neutroLoss = pawn.health.hediffSet.GetFirstHediffOfDef(VREAndroidsDefOf.VREA_NeutroLoss);
                if (neutroLoss != null)
                {
                    neutroLoss.Severity = 0f;
                }
            }
        }
    }
}
