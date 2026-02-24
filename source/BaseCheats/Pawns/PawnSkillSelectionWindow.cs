using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnSkillSelectionOption
    {
        public PawnSkillSelectionOption(SkillDef skillDef, string displayLabel)
        {
            SkillDef = skillDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public SkillDef SkillDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnSkillSelectionWindow : SearchableSelectionWindow<PawnSkillSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetSkill.SearchField";

        private readonly Action<PawnSkillSelectionOption> onSkillSelected;
        private readonly List<PawnSkillSelectionOption> allOptions;

        public PawnSkillSelectionWindow(Action<PawnSkillSelectionOption> onSkillSelected)
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

        protected override IReadOnlyList<PawnSkillSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnSkillSelectionOption option)
        {
            SkillDef skillDef = option?.SkillDef;
            if (skillDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetSkill.SkillWindow.InfoLine".Translate(skillDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnSkillSelectionOption option, string needle)
        {
            SkillDef skillDef = option?.SkillDef;
            if (skillDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string skillLabel = (skillDef.label ?? string.Empty).ToLowerInvariant();
            string defName = skillDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || skillLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(PawnSkillSelectionOption option)
        {
            Close();
            onSkillSelected?.Invoke(option);
        }

        private static List<PawnSkillSelectionOption> BuildSkillList()
        {
            List<PawnSkillSelectionOption> result = new List<PawnSkillSelectionOption>();
            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                if (skillDef == null)
                {
                    continue;
                }

                string label = skillDef.label.NullOrEmpty() ? skillDef.defName : skillDef.label.CapitalizeFirst();
                result.Add(new PawnSkillSelectionOption(skillDef, label));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.SkillDef.defName)
                .ToList();
        }
    }
}
