using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnTameAnimalCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnTameAnimal",
                "CheatMenu.Cheat.PawnTameAnimal.Label",
                "CheatMenu.Cheat.PawnTameAnimal.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        TameTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnTameAnimal.Message.SelectPawn",
                        repeatTargeting: true));
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

        private static void TameTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnTameAnimal.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.Faction == Faction.OfPlayer)
            {
                CheatMessageService.Message("CheatMenu.PawnTameAnimal.Message.AlreadyPlayer".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (!pawn.AnimalOrWildMan())
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnTameAnimal.Message.NotAnimalOrWildMan".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Map map = pawn.MapHeld;
            Pawn tamer = map?.mapPawns?.FreeColonists?.FirstOrDefault();
            if (tamer == null)
            {
                CheatMessageService.Message("CheatMenu.PawnTameAnimal.Message.NoTamer".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                InteractionWorker_RecruitAttempt.DoRecruit(tamer, pawn);
                DebugActionsUtility.DustPuffFrom(pawn);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to tame animal '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnTameAnimal.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            bool tamed = pawn.Faction == Faction.OfPlayer;
            CheatMessageService.Message(
                tamed
                    ? "CheatMenu.PawnTameAnimal.Message.Result".Translate(pawn.LabelShortCap)
                    : "CheatMenu.PawnTameAnimal.Message.NotTamed".Translate(pawn.LabelShortCap),
                tamed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
