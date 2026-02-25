using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnHediffSelectionOption
    {
        public PawnHediffSelectionOption(HediffDef hediffDef, string displayLabel)
        {
            HediffDef = hediffDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public HediffDef HediffDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnHediffSelectionWindow : SearchableSelectionWindow<PawnHediffSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnAddHediff.SearchField";

        private readonly Action<PawnHediffSelectionOption> onHediffSelected;
        private readonly List<PawnHediffSelectionOption> allOptions;

        public PawnHediffSelectionWindow(Action<PawnHediffSelectionOption> onHediffSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onHediffSelected = onHediffSelected;
            allOptions = BuildHediffList();
        }

        protected override string TitleKey => "CheatMenu.PawnAddHediff.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnAddHediff.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnAddHediff.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnAddHediff.Window.SelectButton";

        protected override IReadOnlyList<PawnHediffSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnHediffSelectionOption option)
        {
            HediffDef hediffDef = option?.HediffDef;
            if (hediffDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnAddHediff.Window.InfoLine".Translate(hediffDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnHediffSelectionOption option, string needle)
        {
            HediffDef hediffDef = option?.HediffDef;
            if (hediffDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string hediffLabel = (hediffDef.label ?? string.Empty).ToLowerInvariant();
            string defName = (hediffDef.defName ?? string.Empty).ToLowerInvariant();
            string className = (hediffDef.hediffClass?.ToStringSafe() ?? string.Empty).ToLowerInvariant();

            return displayLabel.Contains(needle)
                || hediffLabel.Contains(needle)
                || defName.Contains(needle)
                || className.Contains(needle);
        }

        protected override void OnItemSelected(PawnHediffSelectionOption option)
        {
            Close();
            onHediffSelected?.Invoke(option);
        }

        private static List<PawnHediffSelectionOption> BuildHediffList()
        {
            List<PawnHediffSelectionOption> result = new List<PawnHediffSelectionOption>();
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (hediffDef == null)
                {
                    continue;
                }

                string label = hediffDef.LabelCap;
                if (label.NullOrEmpty())
                {
                    label = hediffDef.hediffClass?.ToStringSafe() ?? hediffDef.defName ?? string.Empty;
                }

                if (!hediffDef.debugLabelExtra.NullOrEmpty())
                {
                    label = label + " (" + hediffDef.debugLabelExtra + ")";
                }

                result.Add(new PawnHediffSelectionOption(hediffDef, label));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.HediffDef.defName)
                .ToList();
        }
    }
}
