using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{

    public class PawnSkillSelectionWindow : SearchableSelectionWindow<SkillDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetSkill.SearchField";

        private readonly Action<SkillDef> onSkillSelected;
        private readonly List<SkillDef> allOptions;

        public PawnSkillSelectionWindow(Action<SkillDef> onSkillSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onSkillSelected = onSkillSelected;
            allOptions = BuildSkillList();
        }

        protected override string TitleKey => "CheatMenu.PawnSetSkill.SkillWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetSkill.SkillWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetSkill.SkillWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetSkill.SkillWindow.SelectButton";

        protected override IReadOnlyList<SkillDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, SkillDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.label.CapitalizeFirst());

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetSkill.SkillWindow.InfoLine".Translate(option.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(SkillDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string skillLabel = option.label.ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();

            return skillLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(SkillDef option)
        {
            Close();
            onSkillSelected?.Invoke(option);
        }

        private static List<SkillDef> BuildSkillList()
        {
            List<SkillDef> result = new List<SkillDef>();
            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                result.Add(skillDef);
            }

            return result
                .OrderBy(option => option.label)
                .ThenBy(option => option.defName)
                .ToList();
        }
    }
}
