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
            Find.WindowStack.Add(new PawnSkillSelectionWindow(delegate (SkillDef selected)
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
            if (!context.TryGet(SelectedSkillContextKey, out SkillDef selectedSkill))
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.NoSkillSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!context.TryGet(SelectedPassionContextKey, out PawnPassionSelectionOption selectedPassion))
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

            SkillRecord skill = pawn.skills.GetSkill(selectedSkill);
            if (skill == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetPassion.Message.SkillMissing".Translate(pawn.LabelShortCap, selectedSkill.LabelCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            skill.passion = selectedPassion.Passion;

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnSetPassion.Message.Result".Translate(pawn.LabelShortCap, selectedSkill.LabelCap, selectedPassion.DisplayLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
