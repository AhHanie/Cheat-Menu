using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class InspirationSelectionOption
    {
        public InspirationSelectionOption(InspirationDef inspirationDef, string displayLabel)
        {
            InspirationDef = inspirationDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public InspirationDef InspirationDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnInspirationSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnStartInspiration.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<InspirationSelectionOption> onInspirationSelected;
        private readonly List<InspirationSelectionOption> allOptions;
        private readonly SearchableTableRenderer<InspirationSelectionOption> tableRenderer =
            new SearchableTableRenderer<InspirationSelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnInspirationSelectionWindow(Action<InspirationSelectionOption> onInspirationSelected)
        {
            this.onInspirationSelected = onInspirationSelected;
            allOptions = BuildInspirationList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(860f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnStartInspiration.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnStartInspiration.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawInspirationList(listRect);
        }

        private void DrawInspirationList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearch,
                DrawInspirationRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnStartInspiration.Window.NoMatches".Translate(searchText)));
        }

        private void DrawInspirationRow(Rect rowRect, InspirationSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - SelectButtonWidth - 24f, rowRect.height - 12f);

            DrawInspirationInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnStartInspiration.Window.SelectButton".Translate()))
            {
                SelectInspiration(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectInspiration(option);
            }
        }

        private static void DrawInspirationInfo(Rect rect, InspirationSelectionOption option)
        {
            InspirationDef inspirationDef = option?.InspirationDef;
            if (inspirationDef == null)
            {
                return;
            }

            bool canOccurNow = CanOccurNow(inspirationDef);
            string label = option.DisplayLabel;
            if (!canOccurNow)
            {
                label += " [NO]";
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            string availability = canOccurNow
                ? "CheatMenu.PawnStartInspiration.Window.Available".Translate()
                : "CheatMenu.PawnStartInspiration.Window.NotAvailable".Translate();

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnStartInspiration.Window.InfoLine".Translate(inspirationDef.defName, availability));
            Text.Font = GameFont.Small;
        }

        private bool MatchesSearch(InspirationSelectionOption option)
        {
            InspirationDef inspirationDef = option?.InspirationDef;
            if (inspirationDef == null)
            {
                return false;
            }

            if (searchText.NullOrEmpty())
            {
                return true;
            }

            string needle = searchText.Trim().ToLowerInvariant();
            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string inspirationLabel = (inspirationDef.label ?? string.Empty).ToLowerInvariant();
            string defName = inspirationDef.defName.ToLowerInvariant();
            return displayLabel.Contains(needle) || inspirationLabel.Contains(needle) || defName.Contains(needle);
        }

        private void SelectInspiration(InspirationSelectionOption option)
        {
            Close();
            onInspirationSelected?.Invoke(option);
        }

        private static List<InspirationSelectionOption> BuildInspirationList()
        {
            List<InspirationSelectionOption> result = new List<InspirationSelectionOption>();
            foreach (InspirationDef inspirationDef in DefDatabase<InspirationDef>.AllDefsListForReading)
            {
                if (inspirationDef == null || inspirationDef.Worker == null)
                {
                    continue;
                }

                string label = inspirationDef.label ?? inspirationDef.defName ?? string.Empty;
                result.Add(new InspirationSelectionOption(inspirationDef, label));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.InspirationDef.defName)
                .ToList();
        }

        private static bool CanOccurNow(InspirationDef inspirationDef)
        {
            if (inspirationDef?.Worker == null)
            {
                return false;
            }

            Map map = Find.CurrentMap;
            List<Pawn> freeColonists = map?.mapPawns?.FreeColonists;
            if (freeColonists == null || freeColonists.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn pawn = freeColonists[i];
                if (pawn != null && inspirationDef.Worker.InspirationCanOccur(pawn))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
