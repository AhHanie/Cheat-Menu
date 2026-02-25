using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class BackstorySelectionOption
    {
        public BackstorySelectionOption(BackstoryDef backstoryDef, string displayLabel)
        {
            BackstoryDef = backstoryDef;
            DisplayLabel = displayLabel;
        }

        public BackstoryDef BackstoryDef { get; }

        public string DisplayLabel { get; }
    }

    public class PawnBackstorySelectionWindow : SearchableSelectionWindow<BackstorySelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetBackstory.SearchField";

        private readonly Action<BackstorySelectionOption> onBackstorySelected;
        private readonly BackstorySlot slot;
        private readonly List<BackstorySelectionOption> allOptions;

        public PawnBackstorySelectionWindow(BackstorySlot slot, Action<BackstorySelectionOption> onBackstorySelected)
            : base(new Vector2(860f, 700f))
        {
            this.slot = slot;
            this.onBackstorySelected = onBackstorySelected;
            allOptions = BuildBackstoryList(slot);
        }

        protected override string TitleKey => "CheatMenu.PawnSetBackstory.BackstoryWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetBackstory.BackstoryWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetBackstory.BackstoryWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetBackstory.BackstoryWindow.SelectButton";

        protected override IReadOnlyList<BackstorySelectionOption> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(PawnSetBackstoryCheat.GetSlotLabel(slot));
        }

        protected override void DrawItemInfo(Rect rect, BackstorySelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetBackstory.BackstoryWindow.InfoLine".Translate(option.BackstoryDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(BackstorySelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = option.DisplayLabel.ToLowerInvariant();
            string defName = option.BackstoryDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(BackstorySelectionOption option)
        {
            Close();
            onBackstorySelected?.Invoke(option);
        }

        private static List<BackstorySelectionOption> BuildBackstoryList(BackstorySlot slot)
        {
            List<BackstorySelectionOption> result = new List<BackstorySelectionOption>();
            foreach (BackstoryDef backstoryDef in DefDatabase<BackstoryDef>.AllDefsListForReading.Where(b => b.slot == slot))
            {
                string displayLabel = backstoryDef.defName;
                result.Add(new BackstorySelectionOption(backstoryDef, displayLabel));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.BackstoryDef.defName)
                .ToList();
        }
    }
}
