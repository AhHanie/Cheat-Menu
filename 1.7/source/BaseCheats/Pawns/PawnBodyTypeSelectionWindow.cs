using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnBodyTypeSelectionOption
    {
        public PawnBodyTypeSelectionOption(BodyTypeDef bodyTypeDef, string displayLabel)
        {
            BodyTypeDef = bodyTypeDef;
            DisplayLabel = displayLabel;
        }

        public BodyTypeDef BodyTypeDef { get; }

        public string DisplayLabel { get; }
    }

    public class PawnBodyTypeSelectionWindow : SearchableSelectionWindow<BodyTypeDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetBodyType.SearchField";

        private readonly Action<BodyTypeDef> onBodyTypeSelected;
        private readonly List<BodyTypeDef> allOptions;

        public PawnBodyTypeSelectionWindow(Action<BodyTypeDef> onBodyTypeSelected)
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

        protected override IReadOnlyList<BodyTypeDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, BodyTypeDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.defName);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetBodyType.Window.InfoLine".Translate(option.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(BodyTypeDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string defName = option.defName.ToLowerInvariant();

            return defName.Contains(needle);
        }

        protected override void OnItemSelected(BodyTypeDef option)
        {
            Close();
            onBodyTypeSelected?.Invoke(option);
        }

        private static List<BodyTypeDef> BuildBodyTypeList()
        {
            return DefDatabase<BodyTypeDef>.AllDefsListForReading
                .OrderBy(option => option.defName)
                .ToList();
        }
    }
}
