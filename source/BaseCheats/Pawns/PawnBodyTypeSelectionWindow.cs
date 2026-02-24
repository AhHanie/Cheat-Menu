using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnBodyTypeSelectionOption
    {
        public PawnBodyTypeSelectionOption(BodyTypeDef bodyTypeDef, string displayLabel)
        {
            BodyTypeDef = bodyTypeDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public BodyTypeDef BodyTypeDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnBodyTypeSelectionWindow : SearchableSelectionWindow<PawnBodyTypeSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetBodyType.SearchField";

        private readonly Action<PawnBodyTypeSelectionOption> onBodyTypeSelected;
        private readonly List<PawnBodyTypeSelectionOption> allOptions;

        public PawnBodyTypeSelectionWindow(Action<PawnBodyTypeSelectionOption> onBodyTypeSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onBodyTypeSelected = onBodyTypeSelected;
            allOptions = BuildBodyTypeList();
        }

        protected override string TitleKey => "CheatMenu.PawnSetBodyType.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetBodyType.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetBodyType.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetBodyType.Window.SelectButton";

        protected override IReadOnlyList<PawnBodyTypeSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnBodyTypeSelectionOption option)
        {
            BodyTypeDef bodyTypeDef = option?.BodyTypeDef;
            if (bodyTypeDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetBodyType.Window.InfoLine".Translate(bodyTypeDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnBodyTypeSelectionOption option, string needle)
        {
            BodyTypeDef bodyTypeDef = option?.BodyTypeDef;
            if (bodyTypeDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string bodyTypeLabel = (bodyTypeDef.label ?? string.Empty).ToLowerInvariant();
            string defName = bodyTypeDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || bodyTypeLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(PawnBodyTypeSelectionOption option)
        {
            Close();
            onBodyTypeSelected?.Invoke(option);
        }

        private static List<PawnBodyTypeSelectionOption> BuildBodyTypeList()
        {
            return DefDatabase<BodyTypeDef>.AllDefsListForReading
                .Where(bodyTypeDef => bodyTypeDef != null)
                .Select(bodyTypeDef => new PawnBodyTypeSelectionOption(bodyTypeDef, GetDisplayLabel(bodyTypeDef)))
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.BodyTypeDef.defName)
                .ToList();
        }

        private static string GetDisplayLabel(BodyTypeDef bodyTypeDef)
        {
            if (bodyTypeDef == null)
            {
                return string.Empty;
            }

            if (!bodyTypeDef.label.NullOrEmpty())
            {
                return bodyTypeDef.label.CapitalizeFirst();
            }

            return bodyTypeDef.defName ?? string.Empty;
        }
    }
}
