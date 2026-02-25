using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnAddHediffCheat
    {
        private const string SelectedHediffContextKey = "BaseCheats.Pawns.AddHediff.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddHediff",
                "CheatMenu.Cheat.PawnAddHediff.Label",
                "CheatMenu.Cheat.PawnAddHediff.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenHediffSelectionWindow)
                    .AddTool(
                        OpenBodyPartSelectionForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnAddHediff.Message.SelectPawn"));
        }

        private static void OpenHediffSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnHediffSelectionWindow(delegate (PawnHediffSelectionOption selected)
            {
                context.Set(SelectedHediffContextKey, selected);
                continueFlow?.Invoke();
            }));
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

        private static void OpenBodyPartSelectionForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            PawnHediffSelectionOption selected;
            if (!context.TryGet(SelectedHediffContextKey, out selected) || selected?.HediffDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddHediff.Message.NoHediffSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnAddHediff.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.health?.hediffSet == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddHediff.Message.NoHealthTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new PawnHediffBodyPartSelectionWindow(
                pawn,
                selected.HediffDef,
                delegate (PawnHediffBodyPartSelectionOption partSelection)
                {
                    ApplyHediffToPawn(pawn, selected.HediffDef, partSelection);
                }));
        }

        private static void ApplyHediffToPawn(Pawn pawn, HediffDef hediffDef, PawnHediffBodyPartSelectionOption partSelection)
        {
            if (pawn == null || hediffDef == null)
            {
                return;
            }

            BodyPartRecord bodyPart = partSelection?.BodyPart;
            try
            {
                Hediff hediff = bodyPart != null
                    ? HediffMaker.MakeHediff(hediffDef, pawn, bodyPart)
                    : HediffMaker.MakeHediff(hediffDef, pawn);

                if (hediff == null)
                {
                    CheatMessageService.Message(
                        "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnAddHediff.Label".Translate()),
                        MessageTypeDefOf.RejectInput,
                        false);
                    return;
                }

                if (bodyPart != null)
                {
                    pawn.health.AddHediff(hediff, bodyPart);
                }
                else
                {
                    pawn.health.AddHediff(hediff);
                }

                DebugActionsUtility.DustPuffFrom(pawn);
                CheatMessageService.Message(
                    "CheatMenu.PawnAddHediff.Message.Result".Translate(
                        pawn.LabelShortCap,
                        GetHediffDisplayLabel(hediffDef),
                        partSelection?.DisplayLabel ?? "CheatMenu.PawnAddHediff.Part.WholeBody".Translate()),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to add hediff '" + hediffDef.defName + "' to pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnAddHediff.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }

        private static string GetHediffDisplayLabel(HediffDef hediffDef)
        {
            if (hediffDef == null)
            {
                return string.Empty;
            }

            string label = hediffDef.LabelCap;
            if (label.NullOrEmpty())
            {
                label = hediffDef.defName ?? hediffDef.hediffClass?.ToStringSafe() ?? string.Empty;
            }

            if (!hediffDef.debugLabelExtra.NullOrEmpty())
            {
                label = label + " (" + hediffDef.debugLabelExtra + ")";
            }

            return label;
        }
    }
}
