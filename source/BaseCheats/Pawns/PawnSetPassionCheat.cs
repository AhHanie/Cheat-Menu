using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetPassionCheat
    {
        private const string SelectedSkillContextKey = "BaseCheats.Pawns.SetPassion.SelectedSkill";
        private const string SelectedPassionContextKey = "BaseCheats.Pawns.SetPassion.SelectedPassion";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetPassion",
                "CheatMenu.Cheat.PawnSetPassion.Label",
                "CheatMenu.Cheat.PawnSetPassion.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenSkillSelectionWindow)
                    .AddWindow(OpenPassionSelectionWindow)
                    .AddTool(
                        ApplyPassionToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetPassion.Message.SelectPawn"));
        }

        private static void OpenSkillSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnSkillSelectionWindow(delegate (PawnSkillSelectionOption selected)
            {
                context.Set(SelectedSkillContextKey, selected);
                continueFlow?.Invoke();
            }));
        }

        private static void OpenPassionSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnPassionSelectionWindow(delegate (PawnPassionSelectionOption selected)
            {
                context.Set(SelectedPassionContextKey, selected);
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

        private static void ApplyPassionToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            PawnSkillSelectionOption selectedSkill;
            if (!context.TryGet(SelectedSkillContextKey, out selectedSkill) || selectedSkill?.SkillDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.NoSkillSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            PawnPassionSelectionOption selectedPassion;
            if (!context.TryGet(SelectedPassionContextKey, out selectedPassion) || selectedPassion == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.NoPassionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.skills == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.NoSkillsTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            SkillRecord skill = pawn.skills.GetSkill(selectedSkill.SkillDef);
            if (skill == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.SkillMissing".Translate(pawn.LabelShortCap, selectedSkill.DisplayLabel), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                skill.passion = selectedPassion.Passion;

                DebugActionsUtility.DustPuffFrom(pawn);
                CheatMessageService.Message(
                    "CheatMenu.PawnSetPassion.Message.Result".Translate(pawn.LabelShortCap, selectedSkill.DisplayLabel, selectedPassion.DisplayLabel),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to set skill '" + selectedSkill.SkillDef.defName + "' passion to '" + selectedPassion.Passion + "' for pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnSetPassion.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
