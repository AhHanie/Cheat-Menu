using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class IncidentDoIncidentWithPointsCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.DoIncidentWithPoints",
                "CheatMenu.Cheat.DoIncidentWithPoints.Label",
                "CheatMenu.Cheat.DoIncidentWithPoints.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Incidents")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .VisibleWhen(IncidentDoIncidentCheat.IsVisibleForCurrentTarget)
                    .AddWindow(OpenIncidentWithPointsWindow));
        }

        private static void OpenIncidentWithPointsWindow(CheatExecutionContext context, Action continueFlow)
        {
            IIncidentTarget target = IncidentDoIncidentCheat.GetTarget();
            if (target == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.NoTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new IncidentWithPointsSelectionWindow(delegate (IncidentDef incidentDef)
            {
                Find.WindowStack.Add(new IncidentPointsSelectionWindow(incidentDef, IncidentDoIncidentCheat.TryExecuteScalableIncidentWithPoints));
            }));
        }
    }
}
