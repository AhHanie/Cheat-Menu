using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentWithPointsSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.Incidents.Points.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 56f;
        private const float RowSpacing = 4f;
        private const float PointsButtonWidth = 128f;

        private readonly Action<IncidentDef> onIncidentSelected;
        private readonly List<IncidentDef> allIncidents;
        private readonly SearchableTableRenderer<IncidentDef> tableRenderer =
            new SearchableTableRenderer<IncidentDef>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public IncidentWithPointsSelectionWindow(Action<IncidentDef> onIncidentSelected)
        {
            this.onIncidentSelected = onIncidentSelected;
            allIncidents = BuildIncidentList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(820f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Incidents.PointsWindow.Title".Translate());
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
                rect => Widgets.Label(rect, "CheatMenu.Incidents.PointsWindow.NoMatches".Translate(searchText)));
        }

        private void DrawIncidentRow(Rect rowRect, IncidentDef incidentDef, bool drawAltBackground)
        {
            if (drawAltBackground)
            {
                Widgets.DrawAltRect(rowRect);
            }

            bool canFireNow = IncidentDoIncidentCheat.CanFireNow(incidentDef);
            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - PointsButtonWidth - 24f, rowRect.height - 12f);
            DrawIncidentInfo(infoRect, incidentDef, canFireNow);

            Rect pointsRect = new Rect(rowRect.xMax - PointsButtonWidth - 8f, rowRect.y + 10f, PointsButtonWidth, rowRect.height - 20f);
            if (Widgets.ButtonText(pointsRect, "CheatMenu.Incidents.Window.PointsButton".Translate()))
            {
                SelectIncident(incidentDef);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectIncident(incidentDef);
            }
        }

        private static void DrawIncidentInfo(Rect rect, IncidentDef incidentDef, bool canFireNow)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), incidentDef.LabelCap);

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

            return incidentDef.label.ToLowerInvariant().Contains(needle)
                || incidentDef.defName.ToLowerInvariant().Contains(needle);
        }

        private void SelectIncident(IncidentDef incidentDef)
        {
            Close();
            onIncidentSelected?.Invoke(incidentDef);
        }

        private static List<IncidentDef> BuildIncidentList()
        {
            return DefDatabase<IncidentDef>.AllDefsListForReading
                .Where(IncidentDoIncidentCheat.SupportsIncidentPoints)
                .OrderBy(def => def.defName)
                .ToList();
        }
    }
}
