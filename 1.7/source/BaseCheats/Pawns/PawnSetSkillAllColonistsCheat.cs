using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetSkillAllColonistsCheat
    {
        private const string SelectedSkillContextKey = "BaseCheats.Pawns.SetSkillAllColonists.SelectedSkill";
        private const string SelectedLevelContextKey = "BaseCheats.Pawns.SetSkillAllColonists.SelectedLevel";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetSkillAllColonists",
                "CheatMenu.Cheat.PawnSetSkillAllColonists.Label",
                "CheatMenu.Cheat.PawnSetSkillAllColonists.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenSkillSelectionWindow)
                    .AddWindow(OpenLevelSelectionWindow)
                    .AddAction(ApplySkillLevelToAllColonists));
        }

        private static void OpenSkillSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnSkillSelectionWindow(delegate (SkillDef selected)
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

        private static void ApplySkillLevelToAllColonists(CheatExecutionContext context)
        {
            if (!context.TryGet(SelectedSkillContextKey, out SkillDef selectedSkill))
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkillAllColonists.Message.NoSkillSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!context.TryGet(SelectedLevelContextKey, out PawnSkillLevelSelectionOption selectedLevel))
            {
                CheatMessageService.Message("CheatMenu.PawnSetSkillAllColonists.Message.NoLevelSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> colonists = Find.CurrentMap.mapPawns.FreeColonists;
            int affected = 0;

            foreach (Pawn pawn in colonists)
            {
                if (pawn.Dead || pawn.skills == null) continue;

                SkillRecord skill = pawn.skills.GetSkill(selectedSkill);
                if (skill == null) continue;

                skill.Level = selectedLevel.Level;
                skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
                DebugActionsUtility.DustPuffFrom(pawn);
                affected++;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnSetSkillAllColonists.Message.Result".Translate(selectedSkill.label.CapitalizeFirst(), selectedLevel.Level, affected),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
