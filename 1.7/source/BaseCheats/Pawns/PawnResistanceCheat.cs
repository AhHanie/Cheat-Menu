using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class PawnResistanceCheat
    {
        private const string ResistanceAmountContextKey = "BaseCheats.PawnResistance.SelectedAmount";

        public static void Register()
        {
            RegisterResistanceIncrease();
            RegisterResistanceDecrease();
        }

        private static void RegisterResistanceIncrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnResistanceIncrease",
                "CheatMenu.Cheat.PawnResistanceIncrease.Label",
                "CheatMenu.Cheat.PawnResistanceIncrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenResistanceAmountWindow)
                    .AddTool(
                        IncreaseResistanceAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnResistanceIncrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void RegisterResistanceDecrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnResistanceDecrease",
                "CheatMenu.Cheat.PawnResistanceDecrease.Label",
                "CheatMenu.Cheat.PawnResistanceDecrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenResistanceAmountWindow)
                    .AddTool(
                        DecreaseResistanceAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnResistanceDecrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenResistanceAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnResistanceSelectionWindow(
                "CheatMenu.PawnResistance.Window.Title",
                "CheatMenu.PawnResistance.Window.Description",
                initialAmount: 1,
                minAmount: 1,
                maxAmount: 100,
                onConfirm: selectedAmount =>
                {
                    context.Set(ResistanceAmountContextKey, selectedAmount);
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

        private static void IncreaseResistanceAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(ResistanceAmountContextKey, 1);
            ApplyResistanceChange(target, selectedAmount, "CheatMenu.PawnResistanceIncrease.Message.Result");
        }

        private static void DecreaseResistanceAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(ResistanceAmountContextKey, 1);
            ApplyResistanceChange(target, -selectedAmount, "CheatMenu.PawnResistanceDecrease.Message.Result");
        }

        private static void ApplyResistanceChange(LocalTargetInfo target, float delta, string resultMessageKey)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnResistance.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.guest == null)
            {
                CheatMessageService.Message("CheatMenu.PawnResistance.Message.NoGuest".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.guest.resistance <= 0f)
            {
                CheatMessageService.Message("CheatMenu.PawnResistance.Message.AlreadyZero".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            pawn.guest.resistance = Mathf.Max(0f, pawn.guest.resistance + delta);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                resultMessageKey.Translate(pawn.LabelShortCap, Mathf.Abs(delta)),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
