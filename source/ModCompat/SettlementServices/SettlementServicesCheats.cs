using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class SettlementServicesCheats
    {
        private const string CategoryKey = "CheatMenu.Category.SettlementServices";
        private const string SelectedJobContextKey = "ModCompat.SettlementServices.SelectedJob";
        private const string SelectedSettlementContextKey = "ModCompat.SettlementServices.SelectedSettlement";
        private const string SelectedSpecialtyContextKey = "ModCompat.SettlementServices.SelectedSpecialty";
        private const string SelectedEventContextKey = "ModCompat.SettlementServices.SelectedEvent";

        private static bool registered;

        public static void Register()
        {
            if (registered)
            {
                return;
            }

            registered = true;
            RegisterCompleteActiveServiceCheat();
            RegisterAddSettlementSpecialtyCheat();
            RegisterForceRandomServiceEventCheat();
            RegisterForceSelectedServiceEventCheat();
        }

        private static void RegisterCompleteActiveServiceCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.SettlementServices.CompleteActiveService",
                "CheatMenu.SettlementServices.Cheat.CompleteActiveService.Label",
                "CheatMenu.SettlementServices.Cheat.CompleteActiveService.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddWindow(OpenJobSelectionWindow)
                    .AddAction(CompleteSelectedJob));
        }

        private static void RegisterAddSettlementSpecialtyCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.SettlementServices.AddSettlementSpecialty",
                "CheatMenu.SettlementServices.Cheat.AddSettlementSpecialty.Label",
                "CheatMenu.SettlementServices.Cheat.AddSettlementSpecialty.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddWindow(OpenSettlementSelectionWindow)
                    .AddWindow(OpenSpecialtySelectionWindow)
                    .AddAction(AddSelectedSpecialty));
        }

        private static void RegisterForceRandomServiceEventCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.SettlementServices.ForceRandomServiceEvent",
                "CheatMenu.SettlementServices.Cheat.ForceRandomServiceEvent.Label",
                "CheatMenu.SettlementServices.Cheat.ForceRandomServiceEvent.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddWindow(OpenJobSelectionWindow)
                    .AddAction(ForceRandomEventOnSelectedJob));
        }

        private static void RegisterForceSelectedServiceEventCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.SettlementServices.ForceSelectedServiceEvent",
                "CheatMenu.SettlementServices.Cheat.ForceSelectedServiceEvent.Label",
                "CheatMenu.SettlementServices.Cheat.ForceSelectedServiceEvent.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddWindow(OpenJobSelectionWindow)
                    .AddWindow(OpenEventSelectionWindow)
                    .AddAction(ForceSelectedEventOnSelectedJob));
        }

        private static void OpenJobSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            List<SettlementServicesJobOption> jobs = SettlementServicesReflection.GetActiveJobs();
            if (jobs.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Job.Message.NoneAvailable".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new SettlementServicesJobSelectionWindow(
                    jobs,
                    selectedJob =>
                    {
                        context.Set(SelectedJobContextKey, selectedJob);
                        continueFlow?.Invoke();
                    }));
        }

        private static void CompleteSelectedJob(CheatExecutionContext context)
        {
            if (!context.TryGet(SelectedJobContextKey, out SettlementServicesJobOption selectedJob))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Job.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!SettlementServicesReflection.TryCompleteJob(selectedJob.JobId))
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.CompleteActiveService.Message.Failed".Translate(selectedJob.JobId),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.SettlementServices.CompleteActiveService.Message.Result".Translate(selectedJob.ServiceLabel, selectedJob.JobId),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void OpenSettlementSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            List<Settlement> settlements = GetEligibleSettlements();
            if (settlements.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Settlement.Message.NoneAvailable".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new SettlementServicesSettlementSelectionWindow(
                    settlements,
                    selectedSettlement =>
                    {
                        context.Set(SelectedSettlementContextKey, selectedSettlement);
                        continueFlow?.Invoke();
                    }));
        }

        private static List<Settlement> GetEligibleSettlements()
        {
            List<Settlement> result = new List<Settlement>();
            List<Settlement> settlementBases = Find.WorldObjects.SettlementBases;
            for (int i = 0; i < settlementBases.Count; i++)
            {
                Settlement settlement = settlementBases[i];
                if (settlement?.Faction == null || settlement.Faction.IsPlayer)
                {
                    continue;
                }

                result.Add(settlement);
            }

            return result;
        }

        private static void OpenSpecialtySelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            if (!context.TryGet(SelectedSettlementContextKey, out Settlement selectedSettlement))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Settlement.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<SettlementServicesSpecialtyOption> specialties = SettlementServicesReflection.GetAddableSpecialties(selectedSettlement);
            if (specialties.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.Specialty.Message.NoneAvailable".Translate(selectedSettlement.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(
                new SettlementServicesSpecialtySelectionWindow(
                    specialties,
                    selectedSpecialty =>
                    {
                        context.Set(SelectedSpecialtyContextKey, selectedSpecialty);
                        continueFlow?.Invoke();
                    }));
        }

        private static void AddSelectedSpecialty(CheatExecutionContext context)
        {
            if (!context.TryGet(SelectedSettlementContextKey, out Settlement selectedSettlement))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Settlement.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!context.TryGet(SelectedSpecialtyContextKey, out SettlementServicesSpecialtyOption selectedSpecialty))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Specialty.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!SettlementServicesReflection.TryAddSpecialty(selectedSettlement, selectedSpecialty))
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.AddSettlementSpecialty.Message.Failed".Translate(selectedSpecialty.Label, selectedSettlement.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.SettlementServices.AddSettlementSpecialty.Message.Result".Translate(selectedSpecialty.Label, selectedSettlement.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void ForceRandomEventOnSelectedJob(CheatExecutionContext context)
        {
            if (!context.TryGet(SelectedJobContextKey, out SettlementServicesJobOption selectedJob))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Job.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!SettlementServicesReflection.TryForceRandomEvent(selectedJob.JobId))
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.ForceRandomServiceEvent.Message.NoneEligible".Translate(selectedJob.JobId),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.SettlementServices.ForceRandomServiceEvent.Message.Result".Translate(selectedJob.JobId),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void OpenEventSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            if (!context.TryGet(SelectedJobContextKey, out SettlementServicesJobOption selectedJob))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Job.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<SettlementServicesEventOption> events = SettlementServicesReflection.GetEligibleEvents(selectedJob.JobId);
            if (events.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.Event.Message.NoneAvailable".Translate(selectedJob.JobId),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Find.WindowStack.Add(
                new SettlementServicesEventSelectionWindow(
                    events,
                    selectedEvent =>
                    {
                        context.Set(SelectedEventContextKey, selectedEvent);
                        continueFlow?.Invoke();
                    }));
        }

        private static void ForceSelectedEventOnSelectedJob(CheatExecutionContext context)
        {
            if (!context.TryGet(SelectedJobContextKey, out SettlementServicesJobOption selectedJob))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Job.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!context.TryGet(SelectedEventContextKey, out SettlementServicesEventOption selectedEvent))
            {
                CheatMessageService.Message("CheatMenu.SettlementServices.Event.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!SettlementServicesReflection.TryForceSpecificEvent(selectedJob.JobId, selectedEvent))
            {
                CheatMessageService.Message(
                    "CheatMenu.SettlementServices.ForceSelectedServiceEvent.Message.Failed".Translate(selectedEvent.Label, selectedJob.JobId),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.SettlementServices.ForceSelectedServiceEvent.Message.Result".Translate(selectedEvent.Label, selectedJob.JobId),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
