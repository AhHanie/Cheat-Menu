using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetSkillCheat
    {
        private const string SelectedSkillContextKey = "BaseCheats.Pawns.SetSkill.SelectedSkill";
        private const string SelectedLevelContextKey = "BaseCheats.Pawns.SetSkill.SelectedLevel";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetSkill",
                "CheatMenu.Cheat.PawnSetSkill.Label",
                "CheatMenu.Cheat.PawnSetSkill.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenSkillSelectionWindow)
                    .AddWindow(OpenLevelSelectionWindow)
                    .AddTool(
                        ApplySkillLevelToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetSkill.Message.SelectPawn"));
        }

        private static void OpenSkillSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnSkillSelectionWindow(delegate (PawnSkillSelectionOption selected)
            {
                context.Set(SelectedSkillContextKey, selected);
                continueFlow?.Invoke();
            }));
        }

        private static void OpenLevelSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnSkillLevelSelectionWindow(delegate (PawnSkillLevelSelectionOption selected)
            {
                context.Set(SelectedLevelContextKey, selected);
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

        private static void ApplySkillLevelToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            PawnSkillSelectionOption selectedSkill;
            if (!context.TryGet(SelectedSkillContextKey, out selectedSkill) || selectedSkill?.SkillDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkill.Message.NoSkillSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            PawnSkillLevelSelectionOption selectedLevel;
            if (!context.TryGet(SelectedLevelContextKey, out selectedLevel) || selectedLevel == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkill.Message.NoLevelSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkill.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.skills == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkill.Message.NoSkillsTracker".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            SkillRecord skill = pawn.skills.GetSkill(selectedSkill.SkillDef);
            if (skill == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkill.Message.SkillMissing".Translate(pawn.LabelShortCap, selectedSkill.DisplayLabel), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                skill.Level = selectedLevel.Level;
                skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;

                DebugActionsUtility.DustPuffFrom(pawn);
                CheatMessageService.Message(
                    "CheatMenu.PawnSetSkill.Message.Result".Translate(pawn.LabelShortCap, selectedSkill.DisplayLabel, selectedLevel.Level),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to set skill '" + selectedSkill.SkillDef.defName + "' to level " + selectedLevel.Level + " for pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnSetSkill.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
