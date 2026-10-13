using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnHeadTypeSelectionWindow : SearchableSelectionWindow<HeadTypeDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetHeadType.SearchField";

        private readonly Action<HeadTypeDef> onHeadTypeSelected;
        private readonly List<HeadTypeDef> allOptions;

        public PawnHeadTypeSelectionWindow(Action<HeadTypeDef> onHeadTypeSelected)
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

        protected override IReadOnlyList<HeadTypeDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, HeadTypeDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.defName);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetHeadType.Window.InfoLine".Translate(option.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(HeadTypeDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string defName = option.defName.ToLowerInvariant();

            return defName.Contains(needle);
        }

        protected override void OnItemSelected(HeadTypeDef option)
        {
            Close();
            onHeadTypeSelected?.Invoke(option);
        }

        private static List<HeadTypeDef> BuildHeadTypeList()
        {
            return DefDatabase<HeadTypeDef>.AllDefsListForReading
                .OrderBy(option => option.defName)
                .ToList();
        }
    }
}
