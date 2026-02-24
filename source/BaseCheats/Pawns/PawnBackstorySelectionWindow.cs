using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class BackstorySelectionOption
    {
        public BackstorySelectionOption(BackstoryDef backstoryDef, string displayLabel)
        {
            BackstoryDef = backstoryDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public BackstoryDef BackstoryDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnBackstorySelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnSetBackstory.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<BackstorySelectionOption> onBackstorySelected;
        private readonly BackstorySlot slot;
        private readonly List<BackstorySelectionOption> allOptions;
        private readonly SearchableTableRenderer<BackstorySelectionOption> tableRenderer =
            new SearchableTableRenderer<BackstorySelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnBackstorySelectionWindow(BackstorySlot slot, Action<BackstorySelectionOption> onBackstorySelected)
        {
            this.slot = slot;
            this.onBackstorySelected = onBackstorySelected;
            allOptions = BuildBackstoryList(slot);

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
            Widgets.Label(
                new Rect(inRect.x, inRect.y, inRect.width, 36f),
                "CheatMenu.PawnSetBackstory.BackstoryWindow.Title".Translate(PawnSetBackstoryCheat.GetSlotLabel(slot)));
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnSetBackstory.BackstoryWindow.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawBackstoryList(listRect);
        }

        private void DrawBackstoryList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearch,
                DrawBackstoryRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnSetBackstory.BackstoryWindow.NoMatches".Translate(searchText)));
        }

        private void DrawBackstoryRow(Rect rowRect, BackstorySelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - SelectButtonWidth - 24f, rowRect.height - 12f);

            DrawBackstoryInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnSetBackstory.BackstoryWindow.SelectButton".Translate()))
            {
                SelectBackstory(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectBackstory(option);
            }
        }

        private static void DrawBackstoryInfo(Rect rect, BackstorySelectionOption option)
        {
            BackstoryDef backstoryDef = option?.BackstoryDef;
            if (backstoryDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetBackstory.BackstoryWindow.InfoLine".Translate(backstoryDef.defName));
            Text.Font = GameFont.Small;
        }

        private bool MatchesSearch(BackstorySelectionOption option)
        {
            BackstoryDef backstoryDef = option?.BackstoryDef;
            if (backstoryDef == null)
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
            string defName = (backstoryDef.defName ?? string.Empty).ToLowerInvariant();

            return displayLabel.Contains(needle) || defName.Contains(needle);
        }

        private void SelectBackstory(BackstorySelectionOption option)
        {
            Close();
            onBackstorySelected?.Invoke(option);
        }

        private static List<BackstorySelectionOption> BuildBackstoryList(BackstorySlot slot)
        {
            List<BackstorySelectionOption> result = new List<BackstorySelectionOption>();
            foreach (BackstoryDef backstoryDef in DefDatabase<BackstoryDef>.AllDefsListForReading.Where(b => b != null && b.slot == slot))
            {
                string displayLabel = backstoryDef.defName;
                result.Add(new BackstorySelectionOption(backstoryDef, displayLabel));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.BackstoryDef.defName)
                .ToList();
        }
    }
}
