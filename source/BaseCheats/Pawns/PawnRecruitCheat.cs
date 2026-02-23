using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnRecruitCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnRecruit",
                "CheatMenu.Cheat.PawnRecruit.Label",
                "CheatMenu.Cheat.PawnRecruit.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        RecruitTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnRecruit.Message.SelectPawn",
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

        private static void RecruitTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnRecruit.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.Faction == Faction.OfPlayer)
            {
                CheatMessageService.Message("CheatMenu.PawnRecruit.Message.AlreadyPlayer".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            try
            {
                if (pawn.RaceProps.Humanlike)
                {
                    Map map = pawn.MapHeld;
                    Pawn recruiter;
                    if (map?.mapPawns?.FreeColonists == null || !map.mapPawns.FreeColonists.TryRandomElement(out recruiter))
                    {
                        CheatMessageService.Message("CheatMenu.PawnRecruit.Message.NoRecruiter".Translate(), MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    InteractionWorker_RecruitAttempt.DoRecruit(recruiter, pawn);
                    DebugActionsUtility.DustPuffFrom(pawn);

                    bool recruited = pawn.Faction == Faction.OfPlayer;
                    CheatMessageService.Message(
                        recruited
                            ? "CheatMenu.PawnRecruit.Message.Result".Translate(pawn.LabelShortCap)
                            : "CheatMenu.PawnRecruit.Message.HumanlikeNotRecruited".Translate(pawn.LabelShortCap),
                        recruited ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                        false);
                    return;
                }

                if (pawn.RaceProps.IsMechanoid)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    DebugActionsUtility.DustPuffFrom(pawn);
                    CheatMessageService.Message(
                        "CheatMenu.PawnRecruit.Message.Result".Translate(pawn.LabelShortCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                    return;
                }

                CheatMessageService.Message(
                    "CheatMenu.PawnRecruit.Message.UnsupportedPawn".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to recruit pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnRecruit.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
