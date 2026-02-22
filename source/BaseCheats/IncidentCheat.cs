using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class IncidentCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.DoIncident",
                "CheatMenu.Cheat.DoIncident.Label",
                "CheatMenu.Cheat.DoIncident.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Incidents")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .VisibleWhen(IsVisibleForCurrentTarget)
                    .AddWindow(OpenIncidentWindow));
        }

        private static void OpenIncidentWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            IIncidentTarget target = GetTarget();
            if (target == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.NoTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new IncidentSelectionWindow(TryExecuteIncident));
        }

        public static bool CanFireNow(IncidentDef incidentDef)
        {
            try
            {
                IIncidentTarget target = GetTarget();
                if (incidentDef == null || target == null || !incidentDef.TargetAllowed(target))
                {
                    return false;
                }

                IncidentParms incidentParms = BuildIncidentParms(incidentDef, target);
                return incidentParms != null && incidentDef.Worker.CanFireNow(incidentParms);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Incident availability check failed.");
                return false;
            }
        }

        public static void TryExecuteIncident(IncidentDef incidentDef)
        {
            if (incidentDef == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.InvalidIncident".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            IIncidentTarget target = GetTarget();
            if (target == null || !incidentDef.TargetAllowed(target))
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TargetNotAllowed".Translate(incidentDef.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            IncidentParms incidentParms;
            try
            {
                incidentParms = BuildIncidentParms(incidentDef, target);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Incident parameter generation failed.");
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.CannotFire".Translate(incidentDef.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if (incidentParms == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.CannotFire".Translate(incidentDef.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            bool executed;
            try
            {
                executed = incidentDef.Worker.TryExecute(incidentParms);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Incident execution failed.");
                executed = false;
            }

            CheatMessageService.Message(
                executed
                    ? "CheatMenu.Incidents.Message.Executed".Translate(incidentDef.LabelCap)
                    : "CheatMenu.Incidents.Message.ExecutionFailed".Translate(incidentDef.LabelCap),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }

        public static bool SupportsRaidPoints(IncidentDef incidentDef)
        {
            if (incidentDef == null)
            {
                return false;
            }

            return incidentDef == IncidentDefOf.RaidEnemy
                || string.Equals(incidentDef.defName, "RaidEnemy", StringComparison.Ordinal);
        }

        public static void TryExecuteIncidentWithPoints(IncidentDef incidentDef, float points)
        {
            if (incidentDef == null)
            {
                CheatMessageService.Message("CheatMenu.Incidents.Message.InvalidIncident".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            IIncidentTarget target = GetTarget();
            if (!(target is Map map) || !incidentDef.TargetAllowed(map))
            {
                CheatMessageService.Message(
                    "CheatMenu.Incidents.Message.TargetNotAllowed".Translate(incidentDef.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            IncidentParms parms = new IncidentParms
            {
                target = map,
                points = points,
                forced = true
            };

            bool executed;
            try
            {
                executed = incidentDef.Worker.TryExecute(parms);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Incident execution with points failed.");
                executed = false;
            }

            CheatMessageService.Message(
                executed
                    ? "CheatMenu.Incidents.Message.ExecutedWithPoints".Translate(incidentDef.LabelCap, points.ToString("F0"))
                    : "CheatMenu.Incidents.Message.ExecutionFailed".Translate(incidentDef.LabelCap),
                executed ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }

        public static IEnumerable<float> RaidPointsOptions(bool extended)
        {
            if (!extended)
            {
                yield return 35f;
                yield return 70f;
                yield return 100f;
                yield return 150f;
                yield return 200f;
                yield return 350f;
                yield return 500f;
                yield return 700f;
                yield return 1000f;
                yield return 1200f;
                yield return 1500f;
                yield return 2000f;
                yield return 3000f;
                yield return 4000f;
                yield return 5000f;
            }
            else
            {
                for (int i = 20; i < 100; i += 10)
                {
                    yield return i;
                }

                for (int i = 100; i < 500; i += 25)
                {
                    yield return i;
                }

                for (int i = 500; i < 1500; i += 50)
                {
                    yield return i;
                }

                for (int i = 1500; i <= 5000; i += 100)
                {
                    yield return i;
                }
            }

            yield return 6000f;
            yield return 7000f;
            yield return 8000f;
            yield return 9000f;
            yield return 10000f;
        }

        private static IncidentParms BuildIncidentParms(IncidentDef incidentDef, IIncidentTarget target)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, target);
            incidentParms.forced = true;

            if (!incidentDef.pointsScaleable || Find.Storyteller == null)
            {
                return incidentParms;
            }

            StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.FirstOrDefault(
                comp => comp is StorytellerComp_OnOffCycle || comp is StorytellerComp_RandomMain);

            if (storytellerComp != null)
            {
                incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
                incidentParms.forced = true;
            }

            return incidentParms;
        }

        public static string GetCurrentTargetLabel()
        {
            IIncidentTarget target = GetTarget();
            if (target == null)
            {
                return "CheatMenu.Incidents.Target.None".Translate().ToString();
            }

            if (target is Map)
            {
                return "CheatMenu.Incidents.Target.Map".Translate().ToString();
            }

            if (target is World)
            {
                return "CheatMenu.Incidents.Target.World".Translate().ToString();
            }

            if (target is Caravan caravan)
            {
                return caravan.LabelCap;
            }

            if (target is WorldObject worldObject)
            {
                return worldObject.LabelCap;
            }

            return target.ToString();
        }

        private static bool IsVisibleForCurrentTarget()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return false;
            }

            IIncidentTarget target = GetTarget();
            if (target == null)
            {
                return false;
            }

            bool isWorld = target is World;
            bool isWorldObject = target is WorldObject;
            if (WorldRendererUtility.WorldSelected)
            {
                return isWorld || isWorldObject;
            }

            return !isWorld && !isWorldObject;
        }

        private static IIncidentTarget GetTarget()
        {
            IIncidentTarget target = WorldRendererUtility.WorldSelected
                ? Find.WorldSelector.SingleSelectedObject as IIncidentTarget
                : null;

            if (target == null && WorldRendererUtility.WorldSelected)
            {
                target = Find.World;
            }

            if (target == null)
            {
                target = Find.CurrentMap;
            }

            return target;
        }
    }
}
