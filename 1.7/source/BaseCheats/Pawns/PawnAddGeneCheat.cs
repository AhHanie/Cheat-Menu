using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnAddGeneCheat
    {
        private const string SelectedGeneContextKey = "BaseCheats.Pawns.AddGene.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddGene",
                "CheatMenu.Cheat.PawnAddGene.Label",
                "CheatMenu.Cheat.PawnAddGene.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireBiotech()
                    .AddWindow(OpenGeneSelectionWindow)
                    .AddTool(
                        ApplyGeneToTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnAddGene.Message.SelectPawn"));
        }

        private static void OpenGeneSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnGeneSelectionWindow(delegate (GeneSelectionOption selected)
            {
                context.Set(SelectedGeneContextKey, selected);
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

        private static void ApplyGeneToTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            GeneSelectionOption selected;
            if (!context.TryGet(SelectedGeneContextKey, out selected) || selected == null || selected.GeneDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.NoGeneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.InvalidTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.genes == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.NoGeneTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.genes.AddGene(selected.GeneDef, selected.IsXenogene);
            CheatMessageService.Message(
                "CheatMenu.PawnAddGene.Message.Result".Translate(pawn.LabelShortCap, selected.DisplayLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
