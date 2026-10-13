using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnDamageCheat
    {
        public static void Register()
        {
            RegisterDamageUntilDown();
            RegisterDamageToDeath();
        }

        private static void RegisterDamageUntilDown()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnDamageUntilDown",
                "CheatMenu.Cheat.PawnDamageUntilDown.Label",
                "CheatMenu.Cheat.PawnDamageUntilDown.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        DamageUntilDown,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnDamageUntilDown.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void RegisterDamageToDeath()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnDamageToDeath",
                "CheatMenu.Cheat.PawnDamageToDeath.Label",
                "CheatMenu.Cheat.PawnDamageToDeath.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        DamageToDeath,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnDamageToDeath.Message.SelectPawn",
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

        private static void DamageUntilDown(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnDamageUntilDown.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnDamageUntilDown.Message.AlreadyDead".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (pawn.Downed)
            {
                CheatMessageService.Message("CheatMenu.PawnDamageUntilDown.Message.AlreadyDowned".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            HealthUtility.DamageUntilDowned(pawn);

            CheatMessageService.Message(
                "CheatMenu.PawnDamageUntilDown.Message.Result".Translate(pawn.LabelShortCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void DamageToDeath(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnDamageToDeath.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnDamageToDeath.Message.AlreadyDead".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            HealthUtility.DamageUntilDead(pawn);

            CheatMessageService.Message(
                "CheatMenu.PawnDamageToDeath.Message.Result".Translate(pawn.LabelShortCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
