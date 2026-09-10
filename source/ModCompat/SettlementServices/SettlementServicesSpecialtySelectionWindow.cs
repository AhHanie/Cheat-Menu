using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class SettlementServicesSpecialtySelectionWindow : SearchableSelectionWindow<SettlementServicesSpecialtyOption>
    {
        private const string SearchControlNameConst = "CheatMenu.SettlementServices.Specialty.SearchField";

        private readonly Action<SettlementServicesSpecialtyOption> onSelected;
        private readonly List<SettlementServicesSpecialtyOption> options;

        public SettlementServicesSpecialtySelectionWindow(List<SettlementServicesSpecialtyOption> options, Action<SettlementServicesSpecialtyOption> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.SettlementServices.Specialty.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.SettlementServices.Specialty.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SettlementServices.Specialty.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SettlementServices.Specialty.Window.SelectButton";

        protected override IReadOnlyList<SettlementServicesSpecialtyOption> Options => options;

        protected override bool MatchesSearch(SettlementServicesSpecialtyOption item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = item.Label != null ? item.Label.ToLowerInvariant() : string.Empty;
            string defName = item.DefName != null ? item.DefName.ToLowerInvariant() : string.Empty;
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, SettlementServicesSpecialtyOption item)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), item.Label);

            Text.Font = GameFont.Tiny;
            string infoLine = "CheatMenu.SettlementServices.Specialty.Window.InfoLine".Translate(item.DefName);
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(SettlementServicesSpecialtyOption item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
