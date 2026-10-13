using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class PawnWillCheat
    {
        private const string WillAmountContextKey = "BaseCheats.PawnWill.SelectedAmount";

        public static void Register()
        {
            RegisterWillIncrease();
            RegisterWillDecrease();
        }

        private static void RegisterWillIncrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnWillIncrease",
                "CheatMenu.Cheat.PawnWillIncrease.Label",
                "CheatMenu.Cheat.PawnWillIncrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenWillAmountWindow)
                    .AddTool(
                        IncreaseWillAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnWillIncrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void RegisterWillDecrease()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnWillDecrease",
                "CheatMenu.Cheat.PawnWillDecrease.Label",
                "CheatMenu.Cheat.PawnWillDecrease.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenWillAmountWindow)
                    .AddTool(
                        DecreaseWillAtTarget,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnWillDecrease.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenWillAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnWillSelectionWindow(
                "CheatMenu.PawnWill.Window.Title",
                "CheatMenu.PawnWill.Window.Description",
                initialAmount: 1,
                minAmount: 1,
                maxAmount: 100,
                onConfirm: selectedAmount =>
                {
                    context.Set(WillAmountContextKey, selectedAmount);
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

        private static void IncreaseWillAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(WillAmountContextKey, 1);
            ApplyWillChange(target, selectedAmount, "CheatMenu.PawnWillIncrease.Message.Result");
        }

        private static void DecreaseWillAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(WillAmountContextKey, 1);
            ApplyWillChange(target, -selectedAmount, "CheatMenu.PawnWillDecrease.Message.Result");
        }

        private static void ApplyWillChange(LocalTargetInfo target, float delta, string resultMessageKey)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnWill.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.guest == null || !pawn.IsPrisoner)
            {
                CheatMessageService.Message("CheatMenu.PawnWill.Message.NotPrisoner".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.guest.will = Mathf.Max(pawn.guest.will + delta, 0f);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                resultMessageKey.Translate(pawn.LabelShortCap, Mathf.Abs(delta)),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
