using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnRelationCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnRelation",
                "CheatMenu.Cheat.PawnRelation.Label",
                "CheatMenu.Cheat.PawnRelation.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenRelationWindowsForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnRelation.Message.SelectPawn"));
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

        private static void OpenRelationWindowsForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnRelation.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!pawn.RaceProps.IsFlesh)
            {
                CheatMessageService.Message("CheatMenu.PawnRelation.Message.RequiresFleshPawn".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new PawnRelationModeSelectionWindow(pawn, add =>
            {
                if (add)
                {
                    OpenAddRelationFlow(pawn);
                    return;
                }

                OpenRemoveRelationWindow(pawn);
            }));
        }

        private static void OpenAddRelationFlow(Pawn pawn)
        {
            Find.WindowStack.Add(new PawnRelationDefSelectionWindow(selectedRelationDef =>
            {
                OpenRelationTargetWindow(pawn, selectedRelationDef);
            }));
        }

        private static void OpenRelationTargetWindow(Pawn pawn, PawnRelationDef relationDef)
        {
            List<Pawn> candidates = PawnRelationTargetPawnSelectionWindow.BuildCandidates(pawn, relationDef);
            if (candidates.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnRelation.Message.NoTargetCandidates".Translate(relationDef.LabelCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            Find.WindowStack.Add(new PawnRelationTargetPawnSelectionWindow(
                pawn,
                relationDef,
                candidates,
                selectedTarget =>
                {
                    pawn.relations.AddDirectRelation(relationDef, selectedTarget);

                    if (relationDef == PawnRelationDefOf.Fiance)
                    {
                        MarriageNameChange nextMarriageNameChange = SpouseRelationUtility.Roll_NameChangeOnMarriage(pawn);
                        pawn.relations.nextMarriageNameChange = nextMarriageNameChange;
                        selectedTarget.relations.nextMarriageNameChange = nextMarriageNameChange;
                    }

                    DebugActionsUtility.DustPuffFrom(pawn);
                    CheatMessageService.Message(
                        "CheatMenu.PawnRelation.Message.Added".Translate(pawn.LabelShortCap, relationDef.LabelCap, selectedTarget.LabelShortCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }));
        }

        private static void OpenRemoveRelationWindow(Pawn pawn)
        {
            if (pawn.relations.DirectRelations.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnRelation.Message.NoDirectRelations".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new PawnRelationRemovalSelectionWindow(pawn, relationToRemove =>
            {
                pawn.relations.RemoveDirectRelation(relationToRemove);
                DebugActionsUtility.DustPuffFrom(pawn);

                CheatMessageService.Message(
                    "CheatMenu.PawnRelation.Message.Removed".Translate(pawn.LabelShortCap, relationToRemove.def.LabelCap, relationToRemove.otherPawn.LabelShortCap),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }));
        }
    }
}
