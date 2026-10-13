using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetXenotypeCheat
    {
        private const string SelectedXenotypeContextKey = "BaseCheats.Pawns.SetXenotype.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetXenotype",
                "CheatMenu.Cheat.PawnSetXenotype.Label",
                "CheatMenu.Cheat.PawnSetXenotype.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireBiotech()
                    .AddWindow(OpenXenotypeSelectionWindow)
                    .AddTool(
                        ApplyXenotypeToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetXenotype.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenXenotypeSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnXenotypeSelectionWindow(delegate (XenotypeDef selected)
            {
                context.Set(SelectedXenotypeContextKey, selected);
                continueFlow?.Invoke();
            }));
        }

        private static TargetingParameters CreatePawnTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = false,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }

        private static void ApplyXenotypeToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedXenotypeContextKey, out XenotypeDef selected))
            {
                CheatMessageService.Message("CheatMenu.PawnSetXenotype.Message.NoXenotypeSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetXenotype.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.genes == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetXenotype.Message.NoGeneTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.genes.SetXenotype(selected);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                "CheatMenu.PawnSetXenotype.Message.Result".Translate(pawn.LabelShortCap, selected.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
