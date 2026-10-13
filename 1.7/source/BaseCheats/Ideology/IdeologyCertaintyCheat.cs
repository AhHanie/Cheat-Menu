using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class IdeologyCheats
    {
        private const string CertaintyReductionPercentContextKey = "BaseCheats.IdeologyCertaintyDecrease.SelectedPercent";

        private static void RegisterCertaintyDecrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.IdeologyCertaintyDecrease",
                "CheatMenu.Ideology.CertaintyDecrease.Label",
                "CheatMenu.Ideology.CertaintyDecrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Ideology")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddWindow(OpenCertaintyAmountWindow)
                    .AddTool(
                        DecreaseCertaintyAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.Ideology.CertaintyDecrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenCertaintyAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.Ideology.Certainty.Window.Title",
                "CheatMenu.Ideology.Certainty.Window.Description",
                initialAmount: 20,
                minAmount: 1,
                maxAmount: 100,
                onConfirm: selectedPercent =>
                {
                    context.Set(CertaintyReductionPercentContextKey, selectedPercent);
                    continueFlow?.Invoke();
                }));
        }

        private static void DecreaseCertaintyAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedPercent = context.Get(CertaintyReductionPercentContextKey, 20);

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.Ideology.CertaintyDecrease.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.ideo == null)
            {
                CheatMessageService.Message("CheatMenu.Ideology.CertaintyDecrease.Message.NoIdeo".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.ideo.Debug_ReduceCertainty(selectedPercent / 100f);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                "CheatMenu.Ideology.CertaintyDecrease.Message.Result".Translate(pawn.LabelShortCap, selectedPercent),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
