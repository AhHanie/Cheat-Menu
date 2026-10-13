using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class Infusion2InfuseSelectionWindow : SearchableSelectionWindow<Def>
    {
        private const string SearchControlNameConst = "CheatMenu.Infusion2.Infuse.SearchField";
        private readonly Action<Def> onSelected;
        private readonly List<Def> options;

        public Infusion2InfuseSelectionWindow(List<Def> options, Action<Def> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.Infusion2.Infuse.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.Infusion2.Infuse.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Infusion2.Infuse.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Infusion2.Infuse.Window.SelectButton";

        protected override IReadOnlyList<Def> Options => options;

        protected override bool MatchesSearch(Def item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = item.label.ToLowerInvariant();
            string defName = item.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, Def item)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), item.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Infusion2.Window.InfoLine".Translate(item.defName));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(Def item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
