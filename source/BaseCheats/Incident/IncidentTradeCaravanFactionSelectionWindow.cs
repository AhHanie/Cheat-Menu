using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentTradeCaravanFactionOption
    {
        public IncidentTradeCaravanFactionOption(Faction faction)
        {
            Faction = faction;
        }

        public Faction Faction { get; }
    }

    public class IncidentTradeCaravanFactionSelectionWindow : SearchableSelectionWindow<IncidentTradeCaravanFactionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.Incidents.TradeCaravanFaction.SearchField";

        private readonly Action<IncidentTradeCaravanFactionOption> onOptionSelected;
        private readonly List<IncidentTradeCaravanFactionOption> options;

        public IncidentTradeCaravanFactionSelectionWindow(
            List<IncidentTradeCaravanFactionOption> options,
            Action<IncidentTradeCaravanFactionOption> onOptionSelected)
            : base(new Vector2(760f, 680f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.options = options ?? new List<IncidentTradeCaravanFactionOption>();
            this.onOptionSelected = onOptionSelected;
        }

        protected override string TitleKey => "CheatMenu.Incidents.TradeCaravanFactionWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.Incidents.TradeCaravanFactionWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Incidents.TradeCaravanFactionWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Incidents.TradeCaravanFactionWindow.SelectButton";

        protected override IReadOnlyList<IncidentTradeCaravanFactionOption> Options => options;

        protected override bool MatchesSearch(IncidentTradeCaravanFactionOption option, string needle)
        {
            if (needle.NullOrEmpty())
            {
                return true;
            }

            string label = option.Faction.Name.ToLowerInvariant();
            string defName = option.Faction.def.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, IncidentTradeCaravanFactionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.Faction.Name);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Incidents.TradeCaravanFactionWindow.InfoLine".Translate(option.Faction.def.defName, option.Faction.def.caravanTraderKinds.Count));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(IncidentTradeCaravanFactionOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }
    }
}
