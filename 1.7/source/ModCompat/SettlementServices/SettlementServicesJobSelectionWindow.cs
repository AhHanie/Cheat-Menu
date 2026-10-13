using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class SettlementServicesJobSelectionWindow : SearchableSelectionWindow<SettlementServicesJobOption>
    {
        private const string SearchControlNameConst = "CheatMenu.SettlementServices.Job.SearchField";

        private readonly Action<SettlementServicesJobOption> onSelected;
        private readonly List<SettlementServicesJobOption> options;

        public SettlementServicesJobSelectionWindow(List<SettlementServicesJobOption> options, Action<SettlementServicesJobOption> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.SettlementServices.Job.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.SettlementServices.Job.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SettlementServices.Job.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SettlementServices.Job.Window.SelectButton";

        protected override IReadOnlyList<SettlementServicesJobOption> Options => options;

        protected override bool MatchesSearch(SettlementServicesJobOption item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            return item.JobId.ToString().Contains(needle)
                || (item.ServiceLabel != null && item.ServiceLabel.ToLowerInvariant().Contains(needle))
                || (item.ServiceDefName != null && item.ServiceDefName.ToLowerInvariant().Contains(needle))
                || (item.SettlementLabel != null && item.SettlementLabel.ToLowerInvariant().Contains(needle))
                || (item.TargetSummary != null && item.TargetSummary.ToLowerInvariant().Contains(needle));
        }

        protected override void DrawItemInfo(Rect rect, SettlementServicesJobOption item)
        {
            Text.Font = GameFont.Small;
            string primaryLabel = "CheatMenu.SettlementServices.Job.Window.PrimaryLine".Translate(
                item.JobId, item.ServiceLabel, item.SettlementLabel);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), primaryLabel);

            Text.Font = GameFont.Tiny;
            string targetText = item.TargetSummary.NullOrEmpty()
                ? "CheatMenu.SettlementServices.Job.Window.NoTarget".Translate().ToString()
                : item.TargetSummary;
            string infoLine = "CheatMenu.SettlementServices.Job.Window.InfoLine".Translate(
                targetText, item.Quantity, BuildEtaText(item.ExpectedCompletionTick));
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        private static string BuildEtaText(int expectedCompletionTick)
        {
            if (expectedCompletionTick < 0)
            {
                return "CheatMenu.SettlementServices.Job.Window.EtaUnknown".Translate();
            }

            int remainingTicks = expectedCompletionTick - Find.TickManager.TicksGame;
            if (remainingTicks <= 0)
            {
                return "CheatMenu.SettlementServices.Job.Window.EtaDue".Translate();
            }

            return remainingTicks.ToStringTicksToPeriod();
        }

        protected override void OnItemSelected(SettlementServicesJobOption item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
