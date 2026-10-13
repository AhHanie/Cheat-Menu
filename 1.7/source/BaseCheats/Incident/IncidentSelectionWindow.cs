using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.Incidents.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 56f;
        private const float RowSpacing = 4f;
        private const float ExecuteButtonWidth = 96f;
        private const float RaidPointsButtonWidth = 138f;
        private const float DropPodRaidButtonWidth = 178f;
        private const float TradeCaravanButtonWidth = 168f;

        private readonly Action<IncidentDef> onIncidentSelected;
        private readonly List<IncidentDef> allIncidents;
        private readonly SearchableTableRenderer<IncidentDef> tableRenderer =
            new SearchableTableRenderer<IncidentDef>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public IncidentSelectionWindow(Action<IncidentDef> onIncidentSelected)
        {
            this.onIncidentSelected = onIncidentSelected;
            allIncidents = BuildIncidentList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(960f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Incidents.Window.Title".Translate());
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(inRect.x, inRect.y + 24f, inRect.width, 24f), "CheatMenu.Incidents.Window.Target".Translate(IncidentDoIncidentCheat.GetCurrentTargetLabel()));

            Rect searchRect = new Rect(inRect.x, inRect.y + 52f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.Incidents.Window.SearchTooltip",
                SearchControlName,
                130f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawIncidentList(listRect);
        }

        private void DrawIncidentList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allIncidents,
                MatchesSearch,
                DrawIncidentRow,
                rect => Widgets.Label(rect, "CheatMenu.Incidents.Window.NoMatches".Translate(searchText)));
        }

        private void DrawIncidentRow(Rect rowRect, IncidentDef incidentDef, bool drawAltBackground)
        {
            if (drawAltBackground)
            {
                Widgets.DrawAltRect(rowRect);
            }

            bool canFireNow = IncidentDoIncidentCheat.CanFireNow(incidentDef);
            bool supportsRaidPoints = IncidentDoIncidentCheat.SupportsRaidPoints(incidentDef);
            bool supportsDropPodRaid = IncidentDropPodRaidAtLocationCheat.SupportsDropPodRaidAtLocation(incidentDef);
            bool supportsTradeCaravan = IncidentTradeCaravanSpecificCheat.SupportsTradeCaravanSpecific(incidentDef);
            Widgets.DrawHighlightIfMouseover(rowRect);

            float rightButtonsWidth = ExecuteButtonWidth + 8f;
            if (supportsRaidPoints)
            {
                rightButtonsWidth += RaidPointsButtonWidth + 6f;
            }
            if (supportsDropPodRaid)
            {
                rightButtonsWidth += DropPodRaidButtonWidth + 6f;
            }
            if (supportsTradeCaravan)
            {
                rightButtonsWidth += TradeCaravanButtonWidth + 6f;
            }

            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - rightButtonsWidth - 16f, rowRect.height - 12f);
            DrawIncidentInfo(infoRect, incidentDef, canFireNow);

            Rect executeRect = new Rect(rowRect.xMax - ExecuteButtonWidth - 8f, rowRect.y + 10f, ExecuteButtonWidth, rowRect.height - 20f);
            if (Widgets.ButtonText(executeRect, "CheatMenu.Incidents.Window.DoButton".Translate()))
            {
                SelectIncident(incidentDef);
            }

            float nextButtonX = executeRect.x - 6f;

            if (supportsRaidPoints)
            {
                Rect raidPointsRect = new Rect(nextButtonX - RaidPointsButtonWidth, executeRect.y, RaidPointsButtonWidth, executeRect.height);
                if (Widgets.ButtonText(raidPointsRect, "CheatMenu.Incidents.Window.RaidPointsButton".Translate()))
                {
                    OpenRaidPointsWindow(incidentDef);
                }

                nextButtonX = raidPointsRect.x - 6f;
            }

            if (supportsDropPodRaid)
            {
                Rect dropPodRaidRect = new Rect(nextButtonX - DropPodRaidButtonWidth, executeRect.y, DropPodRaidButtonWidth, executeRect.height);
                if (Widgets.ButtonText(dropPodRaidRect, "CheatMenu.Incidents.Window.DropPodRaidButton".Translate()))
                {
                    OpenDropPodRaidAtLocationWindow();
                }

                nextButtonX = dropPodRaidRect.x - 6f;
            }

            if (supportsTradeCaravan)
            {
                Rect tradeCaravanRect = new Rect(nextButtonX - TradeCaravanButtonWidth, executeRect.y, TradeCaravanButtonWidth, executeRect.height);
                if (Widgets.ButtonText(tradeCaravanRect, "CheatMenu.Incidents.Window.TradeCaravanButton".Translate()))
                {
                    OpenTradeCaravanSpecificWindow(incidentDef);
                }
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectIncident(incidentDef);
            }
        }

        private static void DrawIncidentInfo(Rect rect, IncidentDef incidentDef, bool canFireNow)
        {
            string label = incidentDef.LabelCap;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            Text.Font = GameFont.Tiny;
            string availability = canFireNow
                ? "CheatMenu.Incidents.Window.Available".Translate()
                : "CheatMenu.Incidents.Window.NotAvailable".Translate();
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Incidents.Window.InfoLine".Translate(incidentDef.defName, availability));
            Text.Font = GameFont.Small;
        }

        private bool MatchesSearch(IncidentDef incidentDef)
        {
            if (searchText.NullOrEmpty())
            {
                return true;
            }

            string needle = searchText.Trim().ToLowerInvariant();
            if (needle.Length == 0)
            {
                return true;
            }

            string label = incidentDef.label.ToLowerInvariant();
            string defName = incidentDef.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        private void SelectIncident(IncidentDef incidentDef)
        {
            Close();
            onIncidentSelected?.Invoke(incidentDef);
        }

        private void OpenRaidPointsWindow(IncidentDef incidentDef)
        {
            Close();
            Find.WindowStack.Add(new RaidPointsSelectionWindow(incidentDef, IncidentDoIncidentCheat.TryExecuteIncidentWithPoints));
        }

        private void OpenTradeCaravanSpecificWindow(IncidentDef incidentDef)
        {
            Close();
            IncidentTradeCaravanSpecificCheat.OpenTradeCaravanSpecificWindow(incidentDef);
        }

        private void OpenDropPodRaidAtLocationWindow()
        {
            Close();
            IncidentDropPodRaidAtLocationCheat.OpenDropPodRaidAtLocationSelector();
        }

        private static List<IncidentDef> BuildIncidentList()
        {
            return DefDatabase<IncidentDef>.AllDefsListForReading
                .OrderBy(def => def.defName)
                .ToList();
        }
    }
}
