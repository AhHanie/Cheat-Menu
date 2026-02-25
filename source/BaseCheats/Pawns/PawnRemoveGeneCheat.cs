using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnRemoveGeneCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnRemoveGene",
                "CheatMenu.Cheat.PawnRemoveGene.Label",
                "CheatMenu.Cheat.PawnRemoveGene.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireBiotech()
                    .AddTool(
                        OpenRemoveGeneOptionsForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnRemoveGene.Message.SelectPawn"));
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

        private static void OpenRemoveGeneOptionsForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnRemoveGene.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.genes == null)
            {
                CheatMessageService.Message("CheatMenu.PawnRemoveGene.Message.NoGeneTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Gene> genes = pawn.genes.GenesListForReading;
            if (genes.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnRemoveGene.Message.NoGenes".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            for (int i = 0; i < genes.Count; i++)
            {
                Gene capturedGene = genes[i];
                options.Add(new DebugMenuOption(capturedGene.LabelCap, DebugMenuOptionMode.Action, delegate
                {
                    pawn.genes.RemoveGene(capturedGene);
                    CheatMessageService.Message(
                        "CheatMenu.PawnRemoveGene.Message.Result".Translate(pawn.LabelShortCap, capturedGene.LabelCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }
    }
}
