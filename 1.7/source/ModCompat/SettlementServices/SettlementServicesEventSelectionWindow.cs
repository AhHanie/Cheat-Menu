using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class SettlementServicesEventSelectionWindow : SearchableSelectionWindow<SettlementServicesEventOption>
    {
        private const string SearchControlNameConst = "CheatMenu.SettlementServices.Event.SearchField";

        private readonly Action<SettlementServicesEventOption> onSelected;
        private readonly List<SettlementServicesEventOption> options;

        public SettlementServicesEventSelectionWindow(List<SettlementServicesEventOption> options, Action<SettlementServicesEventOption> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.SettlementServices.Event.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.SettlementServices.Event.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SettlementServices.Event.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SettlementServices.Event.Window.SelectButton";

        protected override IReadOnlyList<SettlementServicesEventOption> Options => options;

        protected override bool MatchesSearch(SettlementServicesEventOption item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = item.Label != null ? item.Label.ToLowerInvariant() : string.Empty;
            string defName = item.DefName != null ? item.DefName.ToLowerInvariant() : string.Empty;
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, SettlementServicesEventOption item)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), item.Label);

            Text.Font = GameFont.Tiny;
            string infoLine = "CheatMenu.SettlementServices.Event.Window.InfoLine".Translate(item.DefName, item.TriggerPhaseText);
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(SettlementServicesEventOption item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
