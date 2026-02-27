using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralAddTradeShipOfKindContextKey = "BaseCheats.GeneralAddTradeShipOfKind.SelectedTraderKind";

        private static void RegisterAddTradeShipOfKind()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralAddTradeShipOfKind",
                "CheatMenu.General.AddTradeShipOfKind.Label",
                "CheatMenu.General.AddTradeShipOfKind.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenAddTradeShipOfKindWindow)
                    .AddAction(AddTradeShipOfSelectedKind));
        }

        private static void OpenAddTradeShipOfKindWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<GeneralTradeShipTraderKindOption> options = DefDatabase<TraderKindDef>.AllDefsListForReading
                .Where(traderKind => traderKind.orbital)
                .OrderBy(traderKind => traderKind.label)
                .Select(delegate (TraderKindDef traderKind)
                {
                    IncidentParms availabilityParms = StorytellerUtility.DefaultParmsNow(
                        IncidentDefOf.OrbitalTraderArrival.category,
                        Find.CurrentMap);
                    availabilityParms.traderKind = traderKind;

                    bool canFireNow = IncidentDefOf.OrbitalTraderArrival.Worker.CanFireNow(availabilityParms);
                    return new GeneralTradeShipTraderKindOption(traderKind, canFireNow);
                })
                .ToList();

            if (options.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.General.AddTradeShipOfKind.Message.NoTraderKinds".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new GeneralTradeShipTraderKindSelectionWindow(options, delegate (GeneralTradeShipTraderKindOption selectedOption)
            {
                context.Set(GeneralAddTradeShipOfKindContextKey, selectedOption.TraderKindDef);
                continueFlow?.Invoke();
            }));
        }

        private static void AddTradeShipOfSelectedKind(CheatExecutionContext context)
        {
            if (!context.TryGet(GeneralAddTradeShipOfKindContextKey, out TraderKindDef selectedTraderKind))
            {
                CheatMessageService.Message(
                    "CheatMenu.General.AddTradeShipOfKind.Message.NoTraderKindSelected".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Map map = Find.CurrentMap;
            map.passingShipManager.DebugSendAllShipsAway();

            IncidentParms incidentParms = new IncidentParms
            {
                target = map,
                traderKind = selectedTraderKind
            };

            bool executed = IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(incidentParms);
            CheatMessageService.Message(
                executed
                    ? "CheatMenu.General.AddTradeShipOfKind.Message.Executed".Translate(selectedTraderKind.LabelCap)
                    : "CheatMenu.General.AddTradeShipOfKind.Message.ExecutionFailed".Translate(selectedTraderKind.LabelCap),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }
    }
}
