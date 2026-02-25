using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class MapGameConditionDurationSelectionWindow : SearchableSelectionWindow<MapGameConditionDurationOption>
    {
        private readonly Action<MapGameConditionDurationOption> onOptionSelected;
        private readonly List<MapGameConditionDurationOption> options;
        private readonly string conditionLabel;

        public MapGameConditionDurationSelectionWindow(GameConditionDef conditionDef, Action<MapGameConditionDurationOption> onOptionSelected)
            : base(new Vector2(460f, 620f), rowHeight: 46f, rowSpacing: 4f)
        {
            this.onOptionSelected = onOptionSelected;
            conditionLabel = conditionDef.LabelCap;
            options = BuildOptions();
        }

        protected override bool ShowSearchRow => false;

        protected override string TitleKey => "CheatMenu.MapAddGameCondition.DurationWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.MapAddGameCondition.DurationWindow.SearchTooltip";

        protected override string SearchControlName => "CheatMenu.MapAddGameCondition.DurationWindow";

        protected override string NoMatchesKey => "CheatMenu.MapAddGameCondition.DurationWindow.NoOptions";

        protected override string SelectButtonKey => "CheatMenu.MapGameCondition.Window.SelectButton";

        protected override IReadOnlyList<MapGameConditionDurationOption> Options => options;

        protected override TaggedString GetTitleText()
        {
            return "CheatMenu.MapAddGameCondition.DurationWindow.Title".Translate(conditionLabel);
        }

        protected override bool MatchesSearch(MapGameConditionDurationOption item, string needle)
        {
            return true;
        }

        protected override void DrawItemInfo(Rect rect, MapGameConditionDurationOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            TaggedString infoLine = option.IsPermanent
                ? "CheatMenu.MapAddGameCondition.DurationWindow.PermanentInfo".Translate()
                : "CheatMenu.MapAddGameCondition.DurationWindow.TicksInfo".Translate(option.DurationTicks);
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(MapGameConditionDurationOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }

        private static List<MapGameConditionDurationOption> BuildOptions()
        {
            List<MapGameConditionDurationOption> result = new List<MapGameConditionDurationOption>
            {
                new MapGameConditionDurationOption(
                    isPermanent: true,
                    durationTicks: 0,
                    displayLabel: "CheatMenu.MapAddGameCondition.DurationWindow.PermanentOption".Translate().ToString())
            };

            int ticksPerHour = 2500;
            int ticksPerDay = GenDate.TicksPerDay;
            int ticksPerQuadrum = GenDate.TicksPerQuadrum;

            for (int ticks = ticksPerHour; ticks <= ticksPerDay; ticks += ticksPerHour)
            {
                result.Add(new MapGameConditionDurationOption(
                    isPermanent: false,
                    durationTicks: ticks,
                    displayLabel: ticks.ToStringTicksToPeriod() ?? ticks.ToString()));
            }

            for (int ticks = ticksPerDay * 2; ticks <= ticksPerQuadrum; ticks += ticksPerDay)
            {
                result.Add(new MapGameConditionDurationOption(
                    isPermanent: false,
                    durationTicks: ticks,
                    displayLabel: ticks.ToStringTicksToPeriod() ?? ticks.ToString()));
            }

            return result;
        }
    }

    public sealed class MapGameConditionDurationOption
    {
        public MapGameConditionDurationOption(bool isPermanent, int durationTicks, string displayLabel)
        {
            IsPermanent = isPermanent;
            DurationTicks = durationTicks;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public bool IsPermanent { get; }

        public int DurationTicks { get; }

        public string DisplayLabel { get; }
    }
}
