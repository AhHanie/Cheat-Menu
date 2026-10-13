using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnGrantImmunitiesCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnGrantImmunities",
                "CheatMenu.Cheat.PawnGrantImmunities.Label",
                "CheatMenu.Cheat.PawnGrantImmunities.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        GrantImmunitiesAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnGrantImmunities.Message.SelectPawn",
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

        private static void GrantImmunitiesAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnGrantImmunities.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int appliedCount = GrantImmunityForAllHediffs(pawn);
            if (appliedCount <= 0)
            {
                CheatMessageService.Message("CheatMenu.PawnGrantImmunities.Message.NoImmunityRecords".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnGrantImmunities.Message.Result".Translate(pawn.LabelShortCap, appliedCount),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static int GrantImmunityForAllHediffs(Pawn pawn)
        {
            int appliedCount = 0;
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];
                ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(hediff.def);
                if (immunityRecord == null)
                {
                    continue;
                }

                immunityRecord.immunity = 1f;
                appliedCount++;
            }

            return appliedCount;
        }
    }
}
