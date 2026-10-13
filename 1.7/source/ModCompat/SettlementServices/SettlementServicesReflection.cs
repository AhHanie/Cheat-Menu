using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class SettlementServicesReflection
    {
        private const string WorldComponentTypeName = "Settlement_Services.Domain.SettlementServicesWorldComponent";
        private const string JobRecordTypeName = "Settlement_Services.Domain.Records.ServiceJobRecord";
        private const string TargetSnapshotTypeName = "Settlement_Services.Domain.TargetSnapshot";
        private const string SchedulerTypeName = "Settlement_Services.Framework.SettlementServiceJobScheduler";
        private const string SpecialtyServiceTypeName = "Settlement_Services.Framework.Specialty.SettlementSpecialtyService";
        private const string SpecialtyDefTypeName = "Settlement_Services.Framework.Defs.SettlementSpecialtyDef";
        private const string ServiceDefTypeName = "Settlement_Services.Framework.Defs.SettlementServiceDef";
        private const string EventDefTypeName = "Settlement_Services.Framework.Defs.ServiceEventDef";
        private const string JobContextTypeName = "Settlement_Services.Framework.Workers.ServiceJobContext";
        private const string EventRegistryTypeName = "Settlement_Services.Framework.Events.ServiceEventRegistry";
        private const string EventRollServiceTypeName = "Settlement_Services.Framework.Events.ServiceEventRollService";

        private static bool initialized;
        private static bool resolved;

        private static Type worldComponentType;
        private static Type jobRecordType;
        private static Type targetSnapshotType;
        private static Type schedulerType;
        private static Type specialtyServiceType;
        private static Type specialtyDefType;
        private static Type serviceDefType;
        private static Type eventDefType;
        private static Type jobContextType;
        private static Type eventRegistryType;
        private static Type eventRollServiceType;

        private static PropertyInfo currentProperty;
        private static PropertyInfo activeJobsProperty;
        private static MethodInfo getJobMethod;

        private static FieldInfo jobIdField;
        private static FieldInfo serviceDefNameField;
        private static FieldInfo settlementWorldObjectIdField;
        private static FieldInfo targetsField;
        private static FieldInfo quantityField;
        private static FieldInfo expectedCompletionTickField;
        private static FieldInfo eventTargetIndexField;

        private static FieldInfo snapshotLabelField;
        private static FieldInfo snapshotDefNameField;

        private static MethodInfo tryCompleteActiveJobNowMethod;

        private static MethodInfo getSpecialtiesMethod;
        private static MethodInfo tryAddSpecialtyMethod;
        private static FieldInfo specialtyDisabledField;

        private static MethodInfo getNamedSilentFailServiceDefMethod;
        private static FieldInfo eventTriggerPhaseField;

        private static ConstructorInfo jobContextConstructor;
        private static MethodInfo forUnitIndexMethod;

        private static MethodInfo eligibleEventsMethod;
        private static MethodInfo forceRandomMethod;
        private static MethodInfo forceSpecificMethod;

        public static bool IsFullyResolved
        {
            get
            {
                EnsureInitialized();
                return resolved;
            }
        }

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            if (!ResolveTypes() || !ResolveMembers())
            {
                LogResolutionFailure();
                return;
            }

            resolved = true;
        }

        private static bool ResolveTypes()
        {
            worldComponentType = AccessTools.TypeByName(WorldComponentTypeName);
            jobRecordType = AccessTools.TypeByName(JobRecordTypeName);
            targetSnapshotType = AccessTools.TypeByName(TargetSnapshotTypeName);
            schedulerType = AccessTools.TypeByName(SchedulerTypeName);
            specialtyServiceType = AccessTools.TypeByName(SpecialtyServiceTypeName);
            specialtyDefType = AccessTools.TypeByName(SpecialtyDefTypeName);
            serviceDefType = AccessTools.TypeByName(ServiceDefTypeName);
            eventDefType = AccessTools.TypeByName(EventDefTypeName);
            jobContextType = AccessTools.TypeByName(JobContextTypeName);
            eventRegistryType = AccessTools.TypeByName(EventRegistryTypeName);
            eventRollServiceType = AccessTools.TypeByName(EventRollServiceTypeName);

            return worldComponentType != null
                && jobRecordType != null
                && targetSnapshotType != null
                && schedulerType != null
                && specialtyServiceType != null
                && specialtyDefType != null
                && serviceDefType != null
                && eventDefType != null
                && jobContextType != null
                && eventRegistryType != null
                && eventRollServiceType != null
                && typeof(Def).IsAssignableFrom(specialtyDefType)
                && typeof(Def).IsAssignableFrom(serviceDefType)
                && typeof(Def).IsAssignableFrom(eventDefType);
        }

        private static bool ResolveMembers()
        {
            currentProperty = AccessTools.Property(worldComponentType, "Current");
            activeJobsProperty = AccessTools.Property(worldComponentType, "ActiveJobs");
            getJobMethod = AccessTools.Method(worldComponentType, "GetJob", new[] { typeof(int) });

            jobIdField = AccessTools.Field(jobRecordType, "jobId");
            serviceDefNameField = AccessTools.Field(jobRecordType, "serviceDefName");
            settlementWorldObjectIdField = AccessTools.Field(jobRecordType, "settlementWorldObjectId");
            targetsField = AccessTools.Field(jobRecordType, "targets");
            quantityField = AccessTools.Field(jobRecordType, "quantity");
            expectedCompletionTickField = AccessTools.Field(jobRecordType, "expectedCompletionTick");
            eventTargetIndexField = AccessTools.Field(jobRecordType, "eventTargetIndex");

            snapshotLabelField = AccessTools.Field(targetSnapshotType, "snapshotLabel");
            snapshotDefNameField = AccessTools.Field(targetSnapshotType, "snapshotDefName");

            tryCompleteActiveJobNowMethod = AccessTools.Method(schedulerType, "TryCompleteActiveJobNow", new[] { typeof(int) });

            getSpecialtiesMethod = AccessTools.Method(specialtyServiceType, "GetSpecialties", new[] { typeof(Settlement) });
            tryAddSpecialtyMethod = AccessTools.Method(specialtyServiceType, "TryAddSpecialty", new[] { typeof(Settlement), specialtyDefType });
            specialtyDisabledField = AccessTools.Field(specialtyDefType, "disabled");

            Type serviceDefDatabaseType = typeof(DefDatabase<>).MakeGenericType(serviceDefType);
            getNamedSilentFailServiceDefMethod = AccessTools.Method(serviceDefDatabaseType, "GetNamedSilentFail", new[] { typeof(string) });

            eventTriggerPhaseField = AccessTools.Field(eventDefType, "triggerPhase");

            jobContextConstructor = AccessTools.Constructor(jobContextType, new[] { worldComponentType, jobRecordType });
            forUnitIndexMethod = AccessTools.Method(jobContextType, "ForUnitIndex", new[] { typeof(int) });

            eligibleEventsMethod = AccessTools.Method(eventRegistryType, "EligibleEvents", new[] { serviceDefType, jobContextType });
            forceRandomMethod = AccessTools.Method(eventRollServiceType, "ForceRandom", new[] { worldComponentType, serviceDefType, jobRecordType, jobContextType });
            forceSpecificMethod = AccessTools.Method(eventRollServiceType, "ForceSpecific", new[] { worldComponentType, eventDefType, jobRecordType, jobContextType });

            return currentProperty != null
                && activeJobsProperty != null
                && getJobMethod != null
                && jobIdField != null
                && serviceDefNameField != null
                && settlementWorldObjectIdField != null
                && targetsField != null
                && quantityField != null
                && expectedCompletionTickField != null
                && eventTargetIndexField != null
                && snapshotLabelField != null
                && snapshotDefNameField != null
                && tryCompleteActiveJobNowMethod != null
                && getSpecialtiesMethod != null
                && tryAddSpecialtyMethod != null
                && specialtyDisabledField != null
                && getNamedSilentFailServiceDefMethod != null
                && eventTriggerPhaseField != null
                && jobContextConstructor != null
                && forUnitIndexMethod != null
                && eligibleEventsMethod != null
                && forceRandomMethod != null
                && forceSpecificMethod != null;
        }

        private static void LogResolutionFailure()
        {
            UserLogger.Warning(
                "Settlement Services compatibility disabled: the installed Settlement Services build does not expose the expected members. " +
                "The installed Settlement Services build is likely incompatible with this version of Cheat Menu.");
        }

        public static List<SettlementServicesJobOption> GetActiveJobs()
        {
            EnsureInitialized();
            List<SettlementServicesJobOption> result = new List<SettlementServicesJobOption>();
            if (!resolved)
            {
                return result;
            }

            object domain = GetDomain();
            if (domain == null)
            {
                return result;
            }

            if (!(activeJobsProperty.GetValue(domain, null) is IEnumerable activeJobs))
            {
                return result;
            }

            foreach (object job in activeJobs)
            {
                SettlementServicesJobOption option = BuildJobOption(job);
                if (option != null)
                {
                    result.Add(option);
                }
            }

            return result.OrderBy(option => option.JobId).ToList();
        }

        public static bool TryCompleteJob(int jobId)
        {
            EnsureInitialized();
            if (!resolved)
            {
                return false;
            }

            return Invoke(tryCompleteActiveJobNowMethod, null, new object[] { jobId }) is bool success && success;
        }

        public static List<SettlementServicesSpecialtyOption> GetAddableSpecialties(Settlement settlement)
        {
            EnsureInitialized();
            List<SettlementServicesSpecialtyOption> result = new List<SettlementServicesSpecialtyOption>();
            if (!resolved || settlement == null)
            {
                return result;
            }

            HashSet<string> currentDefNames = new HashSet<string>();
            if (Invoke(getSpecialtiesMethod, null, new object[] { settlement }) is IEnumerable currentSpecialties)
            {
                foreach (object specialty in currentSpecialties)
                {
                    if (specialty is Def def)
                    {
                        currentDefNames.Add(def.defName);
                    }
                }
            }

            IEnumerable allSpecialties = GetAllDefs(specialtyDefType);
            if (allSpecialties == null)
            {
                return result;
            }

            foreach (object specialtyObj in allSpecialties)
            {
                if (!(specialtyObj is Def def))
                {
                    continue;
                }

                bool disabled = specialtyDisabledField.GetValue(def) is bool value && value;
                if (disabled || currentDefNames.Contains(def.defName))
                {
                    continue;
                }

                result.Add(new SettlementServicesSpecialtyOption(def, def.defName, def.LabelCap.ToString(), false));
            }

            return result
                .OrderBy(option => option.Label)
                .ThenBy(option => option.DefName)
                .ToList();
        }

        public static bool TryAddSpecialty(Settlement settlement, SettlementServicesSpecialtyOption option)
        {
            EnsureInitialized();
            if (!resolved || settlement == null || option?.SpecialtyDef == null)
            {
                return false;
            }

            return Invoke(tryAddSpecialtyMethod, null, new object[] { settlement, option.SpecialtyDef }) is bool success && success;
        }

        public static List<SettlementServicesEventOption> GetEligibleEvents(int jobId)
        {
            EnsureInitialized();
            List<SettlementServicesEventOption> result = new List<SettlementServicesEventOption>();
            if (!resolved)
            {
                return result;
            }

            object domain = GetDomain();
            object job = domain != null ? getJobMethod.Invoke(domain, new object[] { jobId }) : null;
            if (job == null)
            {
                return result;
            }

            Def serviceDef = GetServiceDef((string)serviceDefNameField.GetValue(job));
            if (serviceDef == null)
            {
                return result;
            }

            object ctx = BuildJobContext(domain, job);
            if (ctx == null)
            {
                return result;
            }

            if (!(Invoke(eligibleEventsMethod, null, new object[] { serviceDef, ctx }) is IEnumerable eligible))
            {
                return result;
            }

            foreach (object eventObj in eligible)
            {
                if (!(eventObj is Def def))
                {
                    continue;
                }

                string triggerPhaseText = eventTriggerPhaseField.GetValue(def)?.ToString() ?? string.Empty;
                result.Add(new SettlementServicesEventOption(def, def.defName, def.LabelCap.ToString(), triggerPhaseText));
            }

            return result
                .OrderBy(option => option.Label)
                .ThenBy(option => option.DefName)
                .ToList();
        }

        public static bool TryForceRandomEvent(int jobId)
        {
            EnsureInitialized();
            if (!resolved)
            {
                return false;
            }

            object domain = GetDomain();
            object job = domain != null ? getJobMethod.Invoke(domain, new object[] { jobId }) : null;
            if (job == null)
            {
                return false;
            }

            Def serviceDef = GetServiceDef((string)serviceDefNameField.GetValue(job));
            if (serviceDef == null)
            {
                return false;
            }

            object ctx = BuildJobContext(domain, job);
            if (ctx == null)
            {
                return false;
            }

            return Invoke(forceRandomMethod, null, new object[] { domain, serviceDef, job, ctx }) is bool success && success;
        }

        public static bool TryForceSpecificEvent(int jobId, SettlementServicesEventOption option)
        {
            EnsureInitialized();
            if (!resolved || option?.EventDef == null)
            {
                return false;
            }

            object domain = GetDomain();
            object job = domain != null ? getJobMethod.Invoke(domain, new object[] { jobId }) : null;
            if (job == null)
            {
                return false;
            }

            object ctx = BuildJobContext(domain, job);
            if (ctx == null)
            {
                return false;
            }

            List<SettlementServicesEventOption> stillEligible = GetEligibleEvents(jobId);
            if (!stillEligible.Any(eligibleOption => eligibleOption.DefName == option.DefName))
            {
                return false;
            }

            Invoke(forceSpecificMethod, null, new object[] { domain, option.EventDef, job, ctx });
            return true;
        }

        private static object GetDomain()
        {
            return currentProperty.GetValue(null, null);
        }

        private static Def GetServiceDef(string defName)
        {
            if (defName.NullOrEmpty())
            {
                return null;
            }

            return Invoke(getNamedSilentFailServiceDefMethod, null, new object[] { defName }) as Def;
        }

        private static object BuildJobContext(object domain, object job)
        {
            object baseContext = jobContextConstructor.Invoke(new[] { domain, job });
            int eventTargetIndex = (int)eventTargetIndexField.GetValue(job);
            return Invoke(forUnitIndexMethod, baseContext, new object[] { eventTargetIndex });
        }

        private static SettlementServicesJobOption BuildJobOption(object job)
        {
            if (job == null)
            {
                return null;
            }

            int jobId = (int)jobIdField.GetValue(job);
            string serviceDefName = (string)serviceDefNameField.GetValue(job);
            int settlementWorldObjectId = (int)settlementWorldObjectIdField.GetValue(job);
            int quantity = (int)quantityField.GetValue(job);
            int expectedCompletionTick = (int)expectedCompletionTickField.GetValue(job);

            Def serviceDef = GetServiceDef(serviceDefName);
            string serviceLabel = serviceDef != null ? serviceDef.LabelCap.ToString() : serviceDefName;

            Settlement settlement = FindSettlement(settlementWorldObjectId);
            string settlementLabel = settlement != null ? settlement.LabelCap.ToString() : settlementWorldObjectId.ToString();

            return new SettlementServicesJobOption(
                jobId,
                serviceDefName,
                serviceLabel,
                settlementWorldObjectId,
                settlementLabel,
                BuildTargetSummary(job),
                quantity,
                expectedCompletionTick);
        }

        private static string BuildTargetSummary(object job)
        {
            if (!(targetsField.GetValue(job) is IEnumerable targets))
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            foreach (object snapshot in targets)
            {
                if (snapshot == null)
                {
                    continue;
                }

                string label = snapshotLabelField.GetValue(snapshot) as string;
                if (label.NullOrEmpty())
                {
                    label = snapshotDefNameField.GetValue(snapshot) as string;
                }

                if (!label.NullOrEmpty())
                {
                    labels.Add(label);
                }
            }

            return labels.Count > 0 ? string.Join(", ", labels) : string.Empty;
        }

        private static Settlement FindSettlement(int settlementWorldObjectId)
        {
            List<Settlement> settlements = Find.WorldObjects.Settlements;
            for (int i = 0; i < settlements.Count; i++)
            {
                if (settlements[i].ID == settlementWorldObjectId)
                {
                    return settlements[i];
                }
            }

            return null;
        }

        private static IEnumerable GetAllDefs(Type defType)
        {
            Type defDatabaseType = typeof(DefDatabase<>).MakeGenericType(defType);
            PropertyInfo allDefsProperty = AccessTools.Property(defDatabaseType, "AllDefsListForReading");
            return allDefsProperty?.GetValue(null, null) as IEnumerable;
        }

        private static object Invoke(MethodInfo method, object instance, object[] args)
        {
            try
            {
                return method.Invoke(instance, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }
    }
}
