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
            if (incidentDef == null || Find.CurrentMap == null || WorldRendererUtility.WorldSelected)
            {
                return false;
            }

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

            if (tradeCaravanIncident == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.InvalidIncident".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map target = Find.CurrentMap;
            if (target == null || !tradeCaravanIncident.TargetAllowed(target))
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
            return IncidentDefOf.TraderCaravanArrival
                ?? DefDatabase<IncidentDef>.GetNamedSilentFail("TraderCaravanArrival");
        }

        private static List<DebugMenuOption> BuildTradeCaravanFactionOptions(IncidentDef incidentDef, Map target)
        {
            List<DebugMenuOption> options = new List<DebugMenuOption>();
            if (Find.FactionManager == null)
            {
                return options;
            }

            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                if (faction?.def?.caravanTraderKinds == null || !faction.def.caravanTraderKinds.Any())
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
            IEnumerable<TraderKindDef> traderKinds = faction?.def?.caravanTraderKinds ?? Enumerable.Empty<TraderKindDef>();
            foreach (TraderKindDef traderKindDef in traderKinds)
            {
                if (traderKindDef == null)
                {
                    continue;
                }

                TraderKindDef capturedTraderKind = traderKindDef;
                string traderKindLabel = capturedTraderKind.label.NullOrEmpty()
                    ? capturedTraderKind.defName
                    : capturedTraderKind.label.CapitalizeFirst();

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
            bool executed;
            try
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
                incidentParms.forced = true;

                if (incidentDef.pointsScaleable && Find.Storyteller != null)
                {
                    StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.FirstOrDefault(
                        comp => comp is StorytellerComp_OnOffCycle || comp is StorytellerComp_RandomMain);
                    if (storytellerComp != null)
                    {
                        incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
                        incidentParms.forced = true;
                    }
                }

                incidentParms.faction = faction;
                incidentParms.traderKind = traderKind;
                executed = incidentDef.Worker.TryExecute(incidentParms);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Trade caravan incident execution failed.");
                executed = false;
            }

            string factionLabel = GetFactionLabel(faction);
            string traderKindLabel = traderKind == null || traderKind.label.NullOrEmpty()
                ? traderKind?.defName ?? "Unknown"
                : traderKind.label.CapitalizeFirst();

            CheatMessageService.Message(
                executed
                    ? "CheatMenu.Incidents.Message.TradeCaravanExecuted".Translate(factionLabel, traderKindLabel)
                    : "CheatMenu.Incidents.Message.TradeCaravanExecutionFailed".Translate(factionLabel, traderKindLabel),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }

        private static string GetFactionLabel(Faction faction)
        {
            if (faction != null && !faction.Name.NullOrEmpty())
            {
                return faction.Name;
            }

            if (faction?.def?.label != null)
            {
                return faction.def.label.CapitalizeFirst();
            }

            return faction?.def?.defName ?? "Unknown";
        }
    }
}
