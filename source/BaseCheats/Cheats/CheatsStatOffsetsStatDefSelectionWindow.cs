using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class CheatsStatOffsetsStatDefSelectionWindow : SearchableSelectionWindow<StatDef>
    {
        private const string SearchControlNameConst = "CheatMenu.Cheats.EditStatOffsets.StatDef.SearchField";

        private readonly Action<StatDef> onStatSelected;
        private readonly List<StatDef> options;

        public CheatsStatOffsetsStatDefSelectionWindow(
            IEnumerable<StatDef> alreadySelected,
            Action<StatDef> onStatSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onStatSelected = onStatSelected;
            options = BuildOptions(alreadySelected);
        }

        protected override string TitleKey => "CheatMenu.Cheats.EditStatOffsets.StatDefWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.Cheats.EditStatOffsets.StatDefWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Cheats.EditStatOffsets.StatDefWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Cheats.EditStatOffsets.StatDefWindow.SelectButton";

        protected override IReadOnlyList<StatDef> Options => options;

        protected override bool MatchesSearch(StatDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = option.LabelCap.ToString().ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, StatDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.LabelCap);

            Text.Font = GameFont.Tiny;
            string categoryLabel = option.category?.label ?? "None";
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Cheats.EditStatOffsets.StatDefWindow.InfoLine".Translate(option.defName, categoryLabel));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(StatDef option)
        {
            Close();
            onStatSelected?.Invoke(option);
        }

        private static List<StatDef> BuildOptions(IEnumerable<StatDef> alreadySelected)
        {
            HashSet<StatDef> selected = new HashSet<StatDef>(alreadySelected ?? Enumerable.Empty<StatDef>());
            List<StatDef> result = new List<StatDef>();

            for (int i = 0; i < DefDatabase<StatDef>.AllDefsListForReading.Count; i++)
            {
                StatDef statDef = DefDatabase<StatDef>.AllDefsListForReading[i];
                if (selected.Contains(statDef))
                {
                    continue;
                }

                result.Add(statDef);
            }

            return result
                .OrderBy(def => def.LabelCap.ToString())
                .ThenBy(def => def.defName)
                .ToList();
        }
    }
}
