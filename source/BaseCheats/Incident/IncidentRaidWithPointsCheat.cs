using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class IncidentRaidWithPointsCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.DoRaidWithPoints",
                "CheatMenu.Cheat.DoRaidWithPoints.Label",
                "CheatMenu.Cheat.DoRaidWithPoints.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Incidents")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(OpenRaidPointsWindow));
        }

        private static void OpenRaidPointsWindow(CheatExecutionContext context)
        {
            IncidentDef raidIncident = IncidentDefOf.RaidEnemy
                ?? DefDatabase<IncidentDef>.GetNamedSilentFail("RaidEnemy");

            if (raidIncident == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.InvalidIncident".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null || !raidIncident.TargetAllowed(map))
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TargetNotAllowed".Translate(raidIncident.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new RaidPointsSelectionWindow(raidIncident, IncidentDoIncidentCheat.TryExecuteIncidentWithPoints));
        }
    }
}
