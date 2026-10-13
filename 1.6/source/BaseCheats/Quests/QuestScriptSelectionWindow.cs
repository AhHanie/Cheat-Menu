using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class QuestScriptSelectionOption
    {
        public QuestScriptSelectionOption(string displayLabel, QuestScriptDef scriptDef, bool isNaturalRandom)
        {
            DisplayLabel = displayLabel;
            ScriptDef = scriptDef;
            IsNaturalRandom = isNaturalRandom;
        }

        public string DisplayLabel { get; }

        public QuestScriptDef ScriptDef { get; }

        public bool IsNaturalRandom { get; }
    }

    public class QuestScriptSelectionWindow : SearchableSelectionWindow<QuestScriptSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.Quests.SearchField";

        private readonly Action<QuestScriptSelectionOption> onOptionSelected;
        private readonly List<QuestScriptSelectionOption> options;

        public QuestScriptSelectionWindow(List<QuestScriptSelectionOption> options, Action<QuestScriptSelectionOption> onOptionSelected)
            : base(new Vector2(960f, 700f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.options = options;
            this.onOptionSelected = onOptionSelected;
        }

        protected override string TitleKey => "CheatMenu.Quests.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.Quests.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Quests.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Quests.Window.SelectButton";

        protected override IReadOnlyList<QuestScriptSelectionOption> Options => options;

        protected override bool MatchesSearch(QuestScriptSelectionOption option, string needle)
        {
            if (needle.NullOrEmpty())
            {
                return true;
            }

            string label = option.DisplayLabel.ToLowerInvariant();
            string defName = option.ScriptDef?.defName?.ToLowerInvariant() ?? string.Empty;
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, QuestScriptSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            string defName = option.ScriptDef?.defName ?? "-";
            string requirements = GetRequirementsLabel(option);
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Quests.Window.InfoLine".Translate(defName, requirements));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(QuestScriptSelectionOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }

        private static string GetRequirementsLabel(QuestScriptSelectionOption option)
        {
            if (option.IsNaturalRandom)
            {
                return "CheatMenu.Quests.Window.Requirements.NaturalRandom".Translate().ToString();
            }

            QuestScriptDef scriptDef = option.ScriptDef;
            if (scriptDef.affectedByPoints && scriptDef.affectedByPopulation)
            {
                return "CheatMenu.Quests.Window.Requirements.PointsAndPopulation".Translate().ToString();
            }

            if (scriptDef.affectedByPoints)
            {
                return "CheatMenu.Quests.Window.Requirements.Points".Translate().ToString();
            }

            if (scriptDef.affectedByPopulation)
            {
                return "CheatMenu.Quests.Window.Requirements.Population".Translate().ToString();
            }

            return "CheatMenu.Quests.Window.Requirements.None".Translate().ToString();
        }
    }
}
