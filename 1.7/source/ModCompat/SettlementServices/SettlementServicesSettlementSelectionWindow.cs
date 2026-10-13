using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class SettlementServicesSettlementSelectionWindow : SearchableSelectionWindow<Settlement>
    {
        private const string SearchControlNameConst = "CheatMenu.SettlementServices.Settlement.SearchField";

        private readonly Action<Settlement> onSelected;
        private readonly List<Settlement> options;

        public SettlementServicesSettlementSelectionWindow(List<Settlement> options, Action<Settlement> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.SettlementServices.Settlement.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.SettlementServices.Settlement.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SettlementServices.Settlement.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SettlementServices.Settlement.Window.SelectButton";

        protected override IReadOnlyList<Settlement> Options => options;

        protected override bool MatchesSearch(Settlement item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = item.LabelCap.ToString().ToLowerInvariant();
            string factionName = item.Faction != null && item.Faction.Name != null ? item.Faction.Name.ToLowerInvariant() : string.Empty;
            return label.Contains(needle) || factionName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, Settlement item)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), item.LabelCap);

            Text.Font = GameFont.Tiny;
            string factionName = item.Faction != null
                ? item.Faction.Name
                : "CheatMenu.SettlementServices.Settlement.Window.NoFaction".Translate().ToString();
            int goodwill = item.Faction != null ? item.Faction.PlayerGoodwill : 0;
            string infoLine = "CheatMenu.SettlementServices.Settlement.Window.InfoLine".Translate(factionName, goodwill);
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(Settlement item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
