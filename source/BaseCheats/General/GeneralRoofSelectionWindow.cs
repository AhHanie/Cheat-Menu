using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class GeneralRoofSelectionOption
    {
        public GeneralRoofSelectionOption(RoofDef roofDef, string displayLabel)
        {
            RoofDef = roofDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public RoofDef RoofDef { get; }

        public string DisplayLabel { get; }

        public bool IsClear => RoofDef == null;
    }

    public class GeneralRoofSelectionWindow : SearchableSelectionWindow<GeneralRoofSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.GeneralEditRoofRect.SearchField";

        private readonly Action<GeneralRoofSelectionOption> onOptionSelected;
        private readonly List<GeneralRoofSelectionOption> allOptions;

        public GeneralRoofSelectionWindow(Action<GeneralRoofSelectionOption> onOptionSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onOptionSelected = onOptionSelected;
            allOptions = BuildOptions();
        }

        protected override string TitleKey => "CheatMenu.GeneralEditRoofRect.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.GeneralEditRoofRect.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.GeneralEditRoofRect.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.GeneralEditRoofRect.Window.SelectButton";

        protected override IReadOnlyList<GeneralRoofSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, GeneralRoofSelectionOption option)
        {
            if (option == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            string infoLine = option.IsClear
                ? "CheatMenu.GeneralEditRoofRect.Window.InfoLineClear".Translate()
                : "CheatMenu.GeneralEditRoofRect.Window.InfoLineRoof".Translate(option.RoofDef.defName);

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(GeneralRoofSelectionOption option, string needle)
        {
            if (option == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string defName = (option.RoofDef?.defName ?? string.Empty).ToLowerInvariant();

            if (option.IsClear)
            {
                return displayLabel.Contains(needle) || "clear".Contains(needle);
            }

            return displayLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(GeneralRoofSelectionOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }

        private static List<GeneralRoofSelectionOption> BuildOptions()
        {
            List<GeneralRoofSelectionOption> result = new List<GeneralRoofSelectionOption>
            {
                new GeneralRoofSelectionOption(
                    roofDef: null,
                    displayLabel: "CheatMenu.GeneralEditRoofRect.ClearOption".Translate())
            };

            result.AddRange(
                DefDatabase<RoofDef>.AllDefsListForReading
                    .Where(roofDef => roofDef != null)
                    .OrderBy(roofDef => roofDef.label ?? roofDef.defName)
                    .Select(roofDef => new GeneralRoofSelectionOption(
                        roofDef,
                        roofDef.LabelCap.ToString())));

            return result;
        }
    }
}
