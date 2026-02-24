using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnHeadTypeSelectionOption
    {
        public PawnHeadTypeSelectionOption(HeadTypeDef headTypeDef, string displayLabel)
        {
            HeadTypeDef = headTypeDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public HeadTypeDef HeadTypeDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnHeadTypeSelectionWindow : SearchableSelectionWindow<PawnHeadTypeSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetHeadType.SearchField";

        private readonly Action<PawnHeadTypeSelectionOption> onHeadTypeSelected;
        private readonly List<PawnHeadTypeSelectionOption> allOptions;

        public PawnHeadTypeSelectionWindow(Action<PawnHeadTypeSelectionOption> onHeadTypeSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onHeadTypeSelected = onHeadTypeSelected;
            allOptions = BuildHeadTypeList();
        }

        protected override string TitleKey => "CheatMenu.PawnSetHeadType.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetHeadType.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetHeadType.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetHeadType.Window.SelectButton";

        protected override IReadOnlyList<PawnHeadTypeSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnHeadTypeSelectionOption option)
        {
            HeadTypeDef headTypeDef = option?.HeadTypeDef;
            if (headTypeDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetHeadType.Window.InfoLine".Translate(headTypeDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnHeadTypeSelectionOption option, string needle)
        {
            HeadTypeDef headTypeDef = option?.HeadTypeDef;
            if (headTypeDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string headTypeLabel = (headTypeDef.label ?? string.Empty).ToLowerInvariant();
            string defName = headTypeDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || headTypeLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(PawnHeadTypeSelectionOption option)
        {
            Close();
            onHeadTypeSelected?.Invoke(option);
        }

        private static List<PawnHeadTypeSelectionOption> BuildHeadTypeList()
        {
            return DefDatabase<HeadTypeDef>.AllDefsListForReading
                .Where(headTypeDef => headTypeDef != null)
                .Select(headTypeDef => new PawnHeadTypeSelectionOption(headTypeDef, GetDisplayLabel(headTypeDef)))
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.HeadTypeDef.defName)
                .ToList();
        }

        private static string GetDisplayLabel(HeadTypeDef headTypeDef)
        {
            if (headTypeDef == null)
            {
                return string.Empty;
            }

            if (!headTypeDef.label.NullOrEmpty())
            {
                return headTypeDef.label.CapitalizeFirst();
            }

            return headTypeDef.defName ?? string.Empty;
        }
    }
}
