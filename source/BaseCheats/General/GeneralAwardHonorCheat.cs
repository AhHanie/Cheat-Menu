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
            List<Faction> factions = GetFactionsWithRoyalTitles();
            if (factions.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.NoFaction".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            for (int i = 0; i < factions.Count; i++)
            {
                Faction faction = factions[i];
                string factionLabel = GetFactionDisplayName(faction);
                options.Add(new FloatMenuOption(factionLabel, delegate
                {
                    context.Set(GeneralAwardHonorFactionContextKey, faction);
                    continueFlow?.Invoke();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
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
            if (!context.TryGet(GeneralAwardHonorFactionContextKey, out faction) || faction == null)
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

            if (pawn.royalty == null)
            {
                CheatMessageService.Message("CheatMenu.GeneralAwardHonor.Message.NoRoyaltyTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                pawn.royalty.GainFavor(faction, amount);

                DebugActionsUtility.DustPuffFrom(pawn);
                CheatMessageService.Message(
                    "CheatMenu.GeneralAwardHonor.Message.Result".Translate(pawn.LabelShortCap, amount, GetFactionDisplayName(faction)),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to award " + amount + " honor from faction '" + GetFactionDisplayName(faction) + "' to pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.GeneralAwardHonor.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }

        private static List<Faction> GetFactionsWithRoyalTitles()
        {
            if (Find.FactionManager == null)
            {
                return new List<Faction>();
            }

            return Find.FactionManager.AllFactionsListForReading
                .Where(faction =>
                    faction != null
                    && !faction.defeated
                    && !faction.IsPlayer
                    && faction.def != null
                    && !faction.def.royalFavorLabel.NullOrEmpty())
                .OrderBy(faction => faction.Name)
                .ToList();
        }

        private static string GetFactionDisplayName(Faction faction)
        {
            if (faction != null && !faction.Name.NullOrEmpty())
            {
                return faction.Name;
            }

            if (faction?.def?.label != null)
            {
                return faction.def.label;
            }

            return faction?.def?.defName ?? "Unknown faction";
        }
    }
}
