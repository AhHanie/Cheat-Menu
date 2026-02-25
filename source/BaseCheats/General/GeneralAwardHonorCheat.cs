using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralAwardHonorAmountContextKey = "BaseCheats.GeneralAwardHonor.SelectedAmount";
        private const string GeneralAwardHonorFactionContextKey = "BaseCheats.GeneralAwardHonor.SelectedFaction";
        private static IEnumerable<Faction> FactionsWithRoyalTitles => Find.FactionManager.AllFactions.Where((Faction f) => f.def.RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0);

        private static void RegisterAwardHonor()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralAwardHonor",
                "CheatMenu.Cheat.GeneralAwardHonor.Label",
                "CheatMenu.Cheat.GeneralAwardHonor.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireRoyalty()
                    .AddWindow(OpenHonorAmountWindow)
                    .AddWindow(OpenHonorFactionWindow)
                    .AddTool(
                        AwardHonorToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.GeneralAwardHonor.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenHonorAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.GeneralAwardHonor.Window.Title",
                "CheatMenu.GeneralAwardHonor.Window.Description",
                initialAmount: 4,
                minAmount: 1,
                maxAmount: 1000,
                onConfirm: selectedAmount =>
                {
                    context.Set(GeneralAwardHonorAmountContextKey, selectedAmount);
                    continueFlow?.Invoke();
                }));
        }

        private static void OpenHonorFactionWindow(CheatExecutionContext context, Action continueFlow)
        {
            if (!FactionsWithRoyalTitles.Any())
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.NoFaction".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new GeneralAwardHonorFactionSelectionWindow(FactionsWithRoyalTitles.ToList(), delegate (Faction selectedFaction)
            {
                context.Set(GeneralAwardHonorFactionContextKey, selectedFaction);
                continueFlow?.Invoke();
            }));
        }

        private static void AwardHonorToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            int amount = context.Get(GeneralAwardHonorAmountContextKey, 0);
            if (amount <= 0)
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.NoAmountSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Faction faction;
            if (!context.TryGet(GeneralAwardHonorFactionContextKey, out faction))
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.NoFactionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.royalty.GainFavor(faction, amount);

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.GeneralAwardHonor.Message.Result".Translate(pawn.LabelShortCap, amount, faction.Name),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
