using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentTradeCaravanTraderKindOption
    {
        public IncidentTradeCaravanTraderKindOption(TraderKindDef traderKindDef, bool canFireNow)
        {
            TraderKindDef = traderKindDef;
            CanFireNow = canFireNow;
        }

        public TraderKindDef TraderKindDef { get; }

        public bool CanFireNow { get; }
    }

    public class IncidentTradeCaravanTraderKindSelectionWindow : SearchableSelectionWindow<IncidentTradeCaravanTraderKindOption>
    {
        private const string SearchControlNameConst = "CheatMenu.Incidents.TradeCaravanTraderKind.SearchField";

        private readonly Faction faction;
        private readonly Action<IncidentTradeCaravanTraderKindOption> onOptionSelected;
        private readonly List<IncidentTradeCaravanTraderKindOption> options;

        public IncidentTradeCaravanTraderKindSelectionWindow(
            Faction faction,
            List<IncidentTradeCaravanTraderKindOption> options,
            Action<IncidentTradeCaravanTraderKindOption> onOptionSelected)
            : base(new Vector2(760f, 680f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.faction = faction;
            this.options = options;
            this.onOptionSelected = onOptionSelected;
        }

        protected override string TitleKey => "CheatMenu.Incidents.TradeCaravanTraderWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.Incidents.TradeCaravanTraderWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Incidents.TradeCaravanTraderWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Incidents.TradeCaravanTraderWindow.SelectButton";

        protected override IReadOnlyList<IncidentTradeCaravanTraderKindOption> Options => options;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(faction.Name);
        }

        protected override bool MatchesSearch(IncidentTradeCaravanTraderKindOption option, string needle)
        {
            if (needle.NullOrEmpty())
            {
                return true;
            }

            string label = option.TraderKindDef.label.ToLowerInvariant();
            string defName = option.TraderKindDef.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, IncidentTradeCaravanTraderKindOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.TraderKindDef.LabelCap);

            Text.Font = GameFont.Tiny;
            string availability = option.CanFireNow
                ? "CheatMenu.Incidents.Window.Available".Translate().ToString()
                : "CheatMenu.Incidents.Window.NotAvailable".Translate().ToString();
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Incidents.TradeCaravanTraderWindow.InfoLine".Translate(option.TraderKindDef.defName, availability));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(IncidentTradeCaravanTraderKindOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }
    }
}
