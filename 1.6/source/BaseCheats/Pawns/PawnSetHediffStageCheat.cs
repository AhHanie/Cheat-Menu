using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetHediffStageCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetHediffStage",
                "CheatMenu.Cheat.PawnSetHediffStage.Label",
                "CheatMenu.Cheat.PawnSetHediffStage.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenHediffSelectionForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetHediffStage.Message.SelectPawn"));
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
                    "CheatMenu.PawnSetHediffStage.Message.InvalidPawnTarget".Translate(),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            List<Hediff> settableHediffs = GetSettableHediffs(pawn);
            if (settableHediffs.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffStage.Message.NoStageableHediffs".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            Find.WindowStack.Add(new PawnCurrentHediffSelectionWindow(
                pawn,
                settableHediffs,
                delegate (Hediff selectedHediff)
                {
                    OpenStageSelectionWindow(pawn, selectedHediff);
                }));
        }

        private static void OpenStageSelectionWindow(Pawn pawn, Hediff hediff)
        {
            Find.WindowStack.Add(new PawnHediffStageSelectionWindow(
                pawn,
                hediff,
                delegate (PawnHediffStageSelectionOption selectedStage)
                {
                    ApplyStage(pawn, hediff, selectedStage);
                }));
        }

        private static void ApplyStage(Pawn pawn, Hediff hediff, PawnHediffStageSelectionOption selectedStage)
        {
            if (!pawn.health.hediffSet.hediffs.Contains(hediff))
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnSetHediffStage.Message.HediffRemoved".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if (hediff is Hediff_Level hediffLevel)
            {
                int targetLevel = Mathf.RoundToInt(selectedStage.MinSeverity);
                targetLevel = Mathf.Clamp(targetLevel, (int)hediff.def.minSeverity, (int)hediff.def.maxSeverity);

                hediffLevel.SetLevelTo(targetLevel);
                hediffLevel.Severity = hediffLevel.level;
                pawn.health.Notify_HediffChanged(hediffLevel);
            }
            else
            {
                hediff.Severity = selectedStage.MinSeverity;
            }

            string hediffDisplayLabel = PawnCurrentHediffSelectionWindow.GetHediffDisplayLabel(hediff);
            string stageLabel = PawnCurrentHediffSelectionWindow.GetStageLabel(selectedStage.Stage, selectedStage.Index);

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnSetHediffStage.Message.Result".Translate(
                    pawn.LabelShortCap,
                    hediffDisplayLabel,
                    selectedStage.Index,
                    stageLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static List<Hediff> GetSettableHediffs(Pawn pawn)
        {
            List<Hediff> result = new List<Hediff>();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def.stages == null || hediff.def.stages.Count <= 1)
                {
                    continue;
                }

                if (hediff is Hediff_Addiction)
                {
                    continue;
                }

                if (!IsStageSettable(hediff))
                {
                    continue;
                }

                result.Add(hediff);
            }
            return result;
        }

        private static bool IsStageSettable(Hediff hediff)
        {
            if (hediff is Hediff_Level)
            {
                return true;
            }

            PropertyInfo prop = hediff.GetType().GetProperty(nameof(Hediff.CurStageIndex));
            if (prop == null)
            {
                return true;
            }

            Type declaringType = prop.DeclaringType;
            return declaringType == typeof(Hediff) || declaringType == typeof(Hediff_Level);
        }
    }
}
