using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetHediffSeverityCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetHediffSeverity",
                "CheatMenu.Cheat.PawnSetHediffSeverity.Label",
                "CheatMenu.Cheat.PawnSetHediffSeverity.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenHediffSelectionForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetHediffSeverity.Message.SelectPawn"));
        }

        private static TargetingParameters CreatePawnTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = false,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }

        private static void OpenHediffSelectionForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffSeverity.Message.InvalidPawnTarget".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            List<Hediff> settableHediffs = GetSettableHediffs(pawn);
            if (settableHediffs.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffSeverity.Message.NoSettableHediffs".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            PawnCurrentHediffSelectionWindowKeys severityKeys = new PawnCurrentHediffSelectionWindowKeys
            {
                TitleKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.Title",
                SearchTooltipKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.SearchTooltip",
                SearchControlName = "CheatMenu.PawnSetHediffSeverity.HediffWindow.SearchField",
                NoMatchesKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.NoMatches",
                SelectButtonKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.SelectButton",
                InfoLineKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.InfoLine",
                BodyPartSuffixKey = "CheatMenu.PawnSetHediffSeverity.HediffWindow.BodyPartSuffix",
            };

            Find.WindowStack.Add(new PawnCurrentHediffSelectionWindow(
                pawn,
                settableHediffs,
                selectedHediff => OpenSeverityWindow(pawn, selectedHediff),
                severityKeys));
        }

        private static void OpenSeverityWindow(Pawn pawn, Hediff hediff)
        {
            Find.WindowStack.Add(new PawnHediffSeveritySelectionWindow(
                pawn,
                hediff,
                selectedSeverity => ApplySeverity(pawn, hediff, selectedSeverity)));
        }

        private static void ApplySeverity(Pawn pawn, Hediff hediff, float selectedSeverity)
        {
            if (!pawn.health.hediffSet.hediffs.Contains(hediff))
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffSeverity.Message.HediffRemoved".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            float clampedSeverity = Mathf.Clamp(selectedSeverity, hediff.def.minSeverity, hediff.def.maxSeverity);
            string hediffDisplayLabel = PawnCurrentHediffSelectionWindow.GetHediffDisplayLabel(hediff);

            if (hediff is Hediff_Level hediffLevel)
            {
                int minLevel = (int)hediff.def.minSeverity;
                int maxLevel = hediff.def.maxSeverity < float.MaxValue ? (int)hediff.def.maxSeverity : int.MaxValue;
                int targetLevel = Mathf.Clamp(Mathf.RoundToInt(clampedSeverity), minLevel, maxLevel);
                hediffLevel.SetLevelTo(targetLevel);
                hediffLevel.Severity = hediffLevel.level;
                pawn.health.Notify_HediffChanged(hediffLevel);
            }
            else
            {
                hediff.Severity = clampedSeverity;
                pawn.health.Notify_HediffChanged(hediff);
            }

            bool wasRemoved = hediff.ShouldRemove;
            if (wasRemoved && pawn.health.hediffSet.hediffs.Contains(hediff))
            {
                pawn.health.RemoveHediff(hediff);
            }

            DebugActionsUtility.DustPuffFrom(pawn);

            if (wasRemoved)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffSeverity.Message.ResultRemoved".Translate(
                        pawn.LabelShortCap,
                        hediffDisplayLabel,
                        clampedSeverity.ToString("0.###")),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            else
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffSeverity.Message.Result".Translate(
                        pawn.LabelShortCap,
                        hediffDisplayLabel,
                        clampedSeverity.ToString("0.###")),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
        }

        private static List<Hediff> GetSettableHediffs(Pawn pawn)
        {
            List<Hediff> result = new List<Hediff>();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (IsSeveritySettable(hediff))
                {
                    result.Add(hediff);
                }
            }
            return result;
        }

        private static bool IsSeveritySettable(Hediff hediff)
        {
            if (hediff.GetType().FullName == "Verse.Hediff_PainField")
            {
                return false;
            }

            PropertyInfo prop = hediff.GetType().GetProperty(nameof(Hediff.Severity));
            if (prop == null)
            {
                return true;
            }

            return prop.GetSetMethod() != null;
        }
    }
}
