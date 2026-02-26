using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class IncidentTradeCaravanSpecificCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.DoTradeCaravanSpecific",
                "CheatMenu.Cheat.DoTradeCaravanSpecific.Label",
                "CheatMenu.Cheat.DoTradeCaravanSpecific.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Incidents")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(OpenTradeCaravanSpecificSelector));
        }

        public static bool SupportsTradeCaravanSpecific(IncidentDef incidentDef)
        {
            return incidentDef == IncidentDefOf.TraderCaravanArrival
                || string.Equals(incidentDef.defName, "TraderCaravanArrival", StringComparison.Ordinal);
        }

        public static void OpenTradeCaravanSpecificWindow(IncidentDef incidentDef)
        {
            IncidentDef tradeCaravanIncident = incidentDef;
            if (!SupportsTradeCaravanSpecific(tradeCaravanIncident))
            {
                tradeCaravanIncident = GetTradeCaravanIncidentDef();
            }

            Map target = Find.CurrentMap;
            if (!tradeCaravanIncident.TargetAllowed(target))
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TargetNotAllowed".Translate(tradeCaravanIncident.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            List<IncidentTradeCaravanFactionOption> factionOptions = BuildTradeCaravanFactionOptions();
            if (factionOptions.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TradeCaravanNoFactions".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new IncidentTradeCaravanFactionSelectionWindow(factionOptions, delegate (IncidentTradeCaravanFactionOption selectedFaction)
            {
                OpenTradeCaravanTraderKindWindow(tradeCaravanIncident, target, selectedFaction.Faction);
            }));
        }

        private static void OpenTradeCaravanSpecificSelector(CheatExecutionContext context)
        {
            OpenTradeCaravanSpecificWindow(GetTradeCaravanIncidentDef());
        }

        private static IncidentDef GetTradeCaravanIncidentDef()
        {
            return IncidentDefOf.TraderCaravanArrival;
        }

        private static List<IncidentTradeCaravanFactionOption> BuildTradeCaravanFactionOptions()
        {
            List<IncidentTradeCaravanFactionOption> options = new List<IncidentTradeCaravanFactionOption>();
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                if (!faction.def.caravanTraderKinds.Any())
                {
                    continue;
                }

                options.Add(new IncidentTradeCaravanFactionOption(faction));
            }

            return options;
        }

        private static void OpenTradeCaravanTraderKindWindow(IncidentDef incidentDef, Map target, Faction faction)
        {
            List<IncidentTradeCaravanTraderKindOption> traderKindOptions = BuildTradeCaravanTraderKindOptions(incidentDef, target, faction);

            if (traderKindOptions.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TradeCaravanNoTraderKinds".Translate(GetFactionLabel(faction)),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new IncidentTradeCaravanTraderKindSelectionWindow(faction, traderKindOptions, delegate (IncidentTradeCaravanTraderKindOption selectedTraderKind)
            {
                TryExecuteTradeCaravanSpecific(incidentDef, target, faction, selectedTraderKind.TraderKindDef);
            }));
        }

        private static List<IncidentTradeCaravanTraderKindOption> BuildTradeCaravanTraderKindOptions(IncidentDef incidentDef, Map target, Faction faction)
        {
            List<IncidentTradeCaravanTraderKindOption> options = new List<IncidentTradeCaravanTraderKindOption>();
            foreach (TraderKindDef traderKindDef in faction.def.caravanTraderKinds)
            {
                IncidentParms availabilityParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
                availabilityParms.faction = faction;
                availabilityParms.traderKind = traderKindDef;

                bool canFireNow = incidentDef.Worker.CanFireNow(availabilityParms);
                options.Add(new IncidentTradeCaravanTraderKindOption(traderKindDef, canFireNow));
            }

            return options;
        }

        private static void TryExecuteTradeCaravanSpecific(IncidentDef incidentDef, Map target, Faction faction, TraderKindDef traderKind)
        {
            
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
            incidentParms.forced = true;

            if (incidentDef.pointsScaleable)
            {
                StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First(
                    comp => comp is StorytellerComp_OnOffCycle || comp is StorytellerComp_RandomMain);
                incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
            }

            incidentParms.faction = faction;
            incidentParms.traderKind = traderKind;
            bool executed = incidentDef.Worker.TryExecute(incidentParms);

            string factionLabel = GetFactionLabel(faction);
            string traderKindLabel = traderKind.label.CapitalizeFirst();

            CheatMessageService.Message(
                executed
                    ? "CheatMenu.Incidents.Message.TradeCaravanExecuted".Translate(factionLabel, traderKindLabel)
                    : "CheatMenu.Incidents.Message.TradeCaravanExecutionFailed".Translate(factionLabel, traderKindLabel),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }

        private static string GetFactionLabel(Faction faction)
        {
            return faction.Name;
        }
    }
}
