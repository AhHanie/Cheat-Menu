using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class IncidentDropPodRaidAtLocationCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.DoDropPodRaidAtLocation",
                "CheatMenu.Cheat.DoDropPodRaidAtLocation.Label",
                "CheatMenu.Cheat.DoDropPodRaidAtLocation.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Incidents")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(OpenDropPodRaidAtLocation));
        }

        public static bool SupportsDropPodRaidAtLocation(IncidentDef incidentDef)
        {
            return IncidentDoIncidentCheat.SupportsRaidPoints(incidentDef);
        }

        public static void OpenDropPodRaidAtLocationSelector()
        {
            Map map = Find.CurrentMap;
            IncidentParms previewParms = CreateBaseParms(map);
            List<IncidentDropPodRaidFactionOption> factionOptions = BuildFactionOptions(previewParms);
            Find.WindowStack.Add(new IncidentDropPodRaidFactionSelectionWindow(factionOptions, delegate (IncidentDropPodRaidFactionOption selectedFaction)
            {
                OpenPointsSelection(selectedFaction.Faction);
            }));
        }

        private static void OpenDropPodRaidAtLocation(CheatExecutionContext context)
        {
            OpenDropPodRaidAtLocationSelector();
        }

        private static void OpenPointsSelection(Faction faction)
        {
            List<float> pointOptions = DebugActionsUtility.PointsOptions(extended: true)
                .OrderBy(x => x)
                .ToList();
            Find.WindowStack.Add(new IncidentDropPodRaidPointsSelectionWindow(faction, pointOptions, delegate (float points)
            {
                OpenRadiusSelection(faction, points);
            }));
        }

        private static void OpenRadiusSelection(Faction faction, float points)
        {
            List<int> radiusOptions = DebugActionsUtility.RadiusOptions()
                .OrderBy(x => x)
                .ToList();
            Find.WindowStack.Add(new IncidentDropPodRaidRadiusSelectionWindow(faction, points, radiusOptions, delegate (int radius)
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate (LocalTargetInfo target)
                {
                    TryExecuteDropPodRaid(faction, points, radius, target.Cell);
                });
            }));
        }

        private static void TryExecuteDropPodRaid(Faction faction, float points, int radius, IntVec3 targetCell)
        {
            Map map = Find.CurrentMap;
            IncidentParms parms = CreateBaseParms(map);
            parms.faction = faction;
            parms.points = points;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.SpecificDropDebug;
            parms.raidStrategy = faction.HostileTo(Faction.OfPlayer)
                ? RaidStrategyDefOf.ImmediateAttack
                : RaidStrategyDefOf.ImmediateAttackFriendly;
            parms.target = map;
            parms.dropInRadius = radius;
            parms.spawnCenter = targetCell;

            IncidentDef raidIncident = GetRaidIncident(faction);
            bool executed = raidIncident.Worker.TryExecute(parms);

            CheatMessageService.Message(
                executed
                    ? "CheatMenu.Incidents.Message.DropPodRaidExecuted".Translate(faction.Name, points.ToString("F0"), radius)
                    : "CheatMenu.Incidents.Message.DropPodRaidExecutionFailed".Translate(faction.Name, points.ToString("F0"), radius),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }

        private static IncidentParms CreateBaseParms(Map map)
        {
            StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First(
                x => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);

            IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, map);
            parms.forced = true;
            return parms;
        }

        private static IncidentDef GetRaidIncident(Faction faction)
        {
            return faction.HostileTo(Faction.OfPlayer)
                ? IncidentDefOf.RaidEnemy
                : IncidentDefOf.RaidFriendly;
        }

        private static List<IncidentDropPodRaidFactionOption> BuildFactionOptions(IncidentParms previewParms)
        {
            List<IncidentDropPodRaidFactionOption> options = new List<IncidentDropPodRaidFactionOption>();
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                IncidentDef raidIncident = GetRaidIncident(faction);
                bool canBeGroupSource = ((IncidentWorker_PawnsArrive)raidIncident.Worker).FactionCanBeGroupSource(faction, previewParms);
                options.Add(new IncidentDropPodRaidFactionOption(faction, canBeGroupSource));
            }

            return options;
        }
    }
}
