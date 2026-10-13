using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSuppressionCheat
    {
        private const string SuppressionPercentContextKey = "BaseCheats.PawnSuppression.SelectedPercent";

        public static void Register()
        {
            RegisterSuppressionIncrease();
            RegisterSuppressionDecrease();
        }

        private static void RegisterSuppressionIncrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSuppressionIncrease",
                "CheatMenu.Cheat.PawnSuppressionIncrease.Label",
                "CheatMenu.Cheat.PawnSuppressionIncrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddWindow(OpenSuppressionPercentWindow)
                    .AddTool(
                        IncreaseSuppressionAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSuppressionIncrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void RegisterSuppressionDecrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSuppressionDecrease",
                "CheatMenu.Cheat.PawnSuppressionDecrease.Label",
                "CheatMenu.Cheat.PawnSuppressionDecrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddWindow(OpenSuppressionPercentWindow)
                    .AddTool(
                        DecreaseSuppressionAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSuppressionDecrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenSuppressionPercentWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnSuppressionPercentSelectionWindow(
                "CheatMenu.PawnSuppression.Window.Title",
                "CheatMenu.PawnSuppression.Window.Description",
                initialPercent: 10,
                minPercent: 1,
                maxPercent: 100,
                onConfirm: selectedPercent =>
                {
                    context.Set(SuppressionPercentContextKey, selectedPercent);
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

        private static void IncreaseSuppressionAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedPercent = context.Get(SuppressionPercentContextKey, 10);

            ApplySuppressionChange(
                target,
                selectedPercent / 100f,
                selectedPercent,
                "CheatMenu.PawnSuppressionIncrease.Message.Result");
        }

        private static void DecreaseSuppressionAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedPercent = context.Get(SuppressionPercentContextKey, 10);

            ApplySuppressionChange(
                target,
                -selectedPercent / 100f,
                selectedPercent,
                "CheatMenu.PawnSuppressionDecrease.Message.Result");
        }

        private static void ApplySuppressionChange(LocalTargetInfo target, float maxSuppressionPercentDelta, int selectedPercent, string resultMessageKey)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSuppression.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.guest == null || !pawn.IsSlave)
            {
                CheatMessageService.Message("CheatMenu.PawnSuppression.Message.NotSlave".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.needs.TryGetNeed(out Need_Suppression suppressionNeed);

            suppressionNeed.CurLevel += suppressionNeed.MaxLevel * maxSuppressionPercentDelta;
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                resultMessageKey.Translate(pawn.LabelShortCap, selectedPercent),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
