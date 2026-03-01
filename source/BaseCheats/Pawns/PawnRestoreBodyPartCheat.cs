using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnRestoreBodyPartCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnRestoreBodyPart",
                "CheatMenu.Cheat.PawnRestoreBodyPart.Label",
                "CheatMenu.Cheat.PawnRestoreBodyPart.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenBodyPartSelectionForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnRestoreBodyPart.Message.SelectPawn"));
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

        private static void OpenBodyPartSelectionForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnRestoreBodyPart.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!pawn.health.hediffSet.GetNotMissingParts().Any())
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnRestoreBodyPart.Message.NoPartsAvailable".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            Find.WindowStack.Add(new PawnRestoreBodyPartSelectionWindow(
                pawn,
                delegate (BodyPartRecord selectedPart)
                {
                    pawn.health.RestorePart(selectedPart);
                    DebugActionsUtility.DustPuffFrom(pawn);
                    CheatMessageService.Message(
                        "CheatMenu.PawnRestoreBodyPart.Message.Result".Translate(pawn.LabelShortCap, selectedPart.LabelCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }));
        }
    }
}
