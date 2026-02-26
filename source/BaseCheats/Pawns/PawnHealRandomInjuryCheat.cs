using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnHealRandomInjuryCheat
    {
        private const string HealAmountContextKey = "BaseCheats.PawnHealRandomInjury.SelectedAmount";

        public static void Register()
        {
            RegisterHealRandomInjury10();
            RegisterHealRandomInjuryX();
        }

        private static void RegisterHealRandomInjury10()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnHealRandomInjury10",
                "CheatMenu.Cheat.PawnHealRandomInjury10.Label",
                "CheatMenu.Cheat.PawnHealRandomInjury10.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        HealRandomInjury10AtTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnHealRandomInjury10.Message.SelectTarget",
                        repeatTargeting: true));
        }

        private static void RegisterHealRandomInjuryX()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnHealRandomInjuryX",
                "CheatMenu.Cheat.PawnHealRandomInjuryX.Label",
                "CheatMenu.Cheat.PawnHealRandomInjuryX.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenHealAmountWindow)
                    .AddTool(
                        HealRandomInjuryXAtTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnHealRandomInjuryX.Message.SelectTarget",
                        repeatTargeting: true));
        }

        private static void OpenHealAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.PawnHealRandomInjuryX.Window.Title",
                "CheatMenu.PawnHealRandomInjuryX.Window.Description",
                initialAmount: 10,
                minAmount: 1,
                maxAmount: 1000,
                onConfirm: selectedAmount =>
                {
                    context.Set(HealAmountContextKey, selectedAmount);
                    continueFlow?.Invoke();
                }));
        }

        private static TargetingParameters CreatePawnOrCellTargetingParameters(CheatExecutionContext context)
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

        private static void HealRandomInjury10AtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            HealRandomInjuryAtTarget(
                target,
                amount: 10f,
                noPawnMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.NoPawn",
                noHealableMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.NoHealable",
                resultMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.Result",
                includeAmountInResultMessage: false);
        }

        private static void HealRandomInjuryXAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(HealAmountContextKey, 0);

            HealRandomInjuryAtTarget(
                target,
                amount: selectedAmount,
                noPawnMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.NoPawn",
                noHealableMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.NoHealable",
                resultMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.Result",
                includeAmountInResultMessage: true);
        }

        private static void HealRandomInjuryAtTarget(
            LocalTargetInfo target,
            float amount,
            string noPawnMessageKey,
            string noHealableMessageKey,
            string resultMessageKey,
            bool includeAmountInResultMessage)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message(noPawnMessageKey.Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            TryHealRandomInjury(pawn, amount);

            string resultMessage = includeAmountInResultMessage
                ? resultMessageKey.Translate(amount)
                : resultMessageKey.Translate();

            CheatMessageService.Message(
                resultMessage,
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static bool TryHealRandomInjury(Pawn pawn, float amount)
        {
            List<Hediff_Injury> healableInjuries = new List<Hediff_Injury>();
            pawn.health.hediffSet.GetHediffs(
                ref healableInjuries,
                injury => injury.CanHealNaturally() || injury.CanHealFromTending());

            if (!healableInjuries.TryRandomElement(out Hediff_Injury selectedInjury))
            {
                return false;
            }

            selectedInjury.Heal(amount);
            return true;
        }
    }
}
