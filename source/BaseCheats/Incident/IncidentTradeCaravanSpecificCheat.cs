using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
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

            List<DebugMenuOption> factionOptions = BuildTradeCaravanFactionOptions(tradeCaravanIncident, target);
            if (factionOptions.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TradeCaravanNoFactions".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(factionOptions));
        }

        private static void OpenTradeCaravanSpecificSelector(CheatExecutionContext context)
        {
            OpenTradeCaravanSpecificWindow(GetTradeCaravanIncidentDef());
        }

        private static IncidentDef GetTradeCaravanIncidentDef()
        {
            return IncidentDefOf.TraderCaravanArrival;
        }

        private static List<DebugMenuOption> BuildTradeCaravanFactionOptions(IncidentDef incidentDef, Map target)
        {
            List<DebugMenuOption> options = new List<DebugMenuOption>();
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                if (!faction.def.caravanTraderKinds.Any())
                {
                    continue;
                }

                Faction capturedFaction = faction;
                options.Add(new DebugMenuOption(
                    GetFactionLabel(capturedFaction),
                    DebugMenuOptionMode.Action,
                    delegate
                    {
                        OpenTradeCaravanTraderKindWindow(incidentDef, target, capturedFaction);
                    }));
            }

            return options;
        }

        private static void OpenTradeCaravanTraderKindWindow(IncidentDef incidentDef, Map target, Faction faction)
        {
            List<DebugMenuOption> traderKindOptions = new List<DebugMenuOption>();
            IEnumerable<TraderKindDef> traderKinds = faction.def.caravanTraderKinds;
            foreach (TraderKindDef traderKindDef in traderKinds)
            {
                TraderKindDef capturedTraderKind = traderKindDef;
                string traderKindLabel = capturedTraderKind.label.CapitalizeFirst();

                IncidentParms availabilityParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
                availabilityParms.faction = faction;
                availabilityParms.traderKind = capturedTraderKind;

                if (!incidentDef.Worker.CanFireNow(availabilityParms))
                {
                    traderKindLabel += " [NO]";
                }

                traderKindOptions.Add(new DebugMenuOption(
                    traderKindLabel,
                    DebugMenuOptionMode.Action,
                    delegate
                    {
                        TryExecuteTradeCaravanSpecific(incidentDef, target, faction, capturedTraderKind);
                    }));
            }

            if (traderKindOptions.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TradeCaravanNoTraderKinds".Translate(GetFactionLabel(faction)),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(traderKindOptions));
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
