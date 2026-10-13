using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterForceStorytellerIncident()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralForceStorytellerIncident",
                "CheatMenu.General.ForceStorytellerIncident.Label",
                "CheatMenu.General.ForceStorytellerIncident.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .VisibleWhen(IncidentDoIncidentCheat.IsVisibleForCurrentTarget)
                    .AddWindow(OpenForceStorytellerIncidentWindow));
        }

        private static void OpenForceStorytellerIncidentWindow(CheatExecutionContext context, Action continueFlow)
        {
            IIncidentTarget target = IncidentDoIncidentCheat.GetTarget();
            if (target == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.General.ForceStorytellerIncident.Message.NoTarget".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(new IncidentSelectionWindow(TryForceStorytellerIncident));
        }

        private static void TryForceStorytellerIncident(IncidentDef incidentDef)
        {
            IIncidentTarget target = IncidentDoIncidentCheat.GetTarget();
            if (target == null || !incidentDef.TargetAllowed(target))
            {
                CheatMessageService.Message(
                    "CheatMenu.General.ForceStorytellerIncident.Message.TargetNotAllowed".Translate(incidentDef.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
            parms.forced = true;
            parms.bypassStorytellerSettings = true;

            bool fired = Find.Storyteller.TryFire(new FiringIncident(incidentDef, null, parms));

            CheatMessageService.Message(
                fired
                    ? "CheatMenu.General.ForceStorytellerIncident.Message.Executed".Translate(incidentDef.LabelCap)
                    : "CheatMenu.General.ForceStorytellerIncident.Message.ExecutionFailed".Translate(incidentDef.LabelCap),
                fired ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }
    }
}
