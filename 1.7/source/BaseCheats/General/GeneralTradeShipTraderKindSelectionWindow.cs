using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class GeneralTradeShipTraderKindOption
    {
        public GeneralTradeShipTraderKindOption(TraderKindDef traderKindDef, bool canFireNow)
        {
            TraderKindDef = traderKindDef;
            CanFireNow = canFireNow;
        }

        public TraderKindDef TraderKindDef { get; }

        public bool CanFireNow { get; }
    }

    public class GeneralTradeShipTraderKindSelectionWindow : SearchableSelectionWindow<GeneralTradeShipTraderKindOption>
    {
        private const string SearchControlNameConst = "CheatMenu.General.AddTradeShipOfKind.SearchField";

        private readonly Action<GeneralTradeShipTraderKindOption> onOptionSelected;
        private readonly List<GeneralTradeShipTraderKindOption> options;

        public GeneralTradeShipTraderKindSelectionWindow(
            List<GeneralTradeShipTraderKindOption> options,
            Action<GeneralTradeShipTraderKindOption> onOptionSelected)
            : base(new Vector2(760f, 680f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.options = options;
            this.onOptionSelected = onOptionSelected;
        }

        protected override string TitleKey => "CheatMenu.General.AddTradeShipOfKind.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.General.AddTradeShipOfKind.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.General.AddTradeShipOfKind.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.General.AddTradeShipOfKind.Window.SelectButton";

        protected override IReadOnlyList<GeneralTradeShipTraderKindOption> Options => options;

        protected override bool MatchesSearch(GeneralTradeShipTraderKindOption option, string needle)
        {
            if (needle.NullOrEmpty())
            {
                return true;
            }

            string label = option.TraderKindDef.label.ToLowerInvariant();
            string defName = option.TraderKindDef.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, GeneralTradeShipTraderKindOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.TraderKindDef.LabelCap);

            Text.Font = GameFont.Tiny;
            string availability = option.CanFireNow
                ? "CheatMenu.Incidents.Window.Available".Translate().ToString()
                : "CheatMenu.Incidents.Window.NotAvailable".Translate().ToString();
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.General.AddTradeShipOfKind.Window.InfoLine".Translate(option.TraderKindDef.defName, availability));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(GeneralTradeShipTraderKindOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }
    }
}
