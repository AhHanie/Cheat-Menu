using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public abstract class SearchableSelectionWindow<TItem> : Window
    {
        private const float DefaultSearchRowHeight = 34f;
        private const float DefaultRowHeight = 54f;
        private const float DefaultRowSpacing = 4f;
        private const float SearchLabelWidth = 132f;
        private const float TitleHeight = 36f;

        private readonly Vector2 initialSize;
        private readonly SearchableTableRenderer<TItem> tableRenderer;

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        protected SearchableSelectionWindow(Vector2 initialSize, float rowHeight = DefaultRowHeight, float rowSpacing = DefaultRowSpacing)
        {
            this.initialSize = initialSize;
            tableRenderer = new SearchableTableRenderer<TItem>(rowHeight, rowSpacing);

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => initialSize;

        protected virtual float SelectButtonWidth => 92f;

        protected virtual float IconSize => 40f;

        protected virtual bool UseIconColumn => false;

        protected virtual bool ShowSearchRow => true;

        protected virtual float SearchRowHeight => DefaultSearchRowHeight;

        protected virtual string SearchLabelKey => "CheatMenu.Window.SearchLabel";

        protected abstract string TitleKey { get; }

        protected abstract string SearchTooltipKey { get; }

        protected abstract string SearchControlName { get; }

        protected abstract string NoMatchesKey { get; }

        protected abstract string SelectButtonKey { get; }

        protected abstract IReadOnlyList<TItem> Options { get; }

        protected abstract bool MatchesSearch(TItem item, string needle);

        protected abstract void DrawItemInfo(Rect rect, TItem item);

        protected abstract void OnItemSelected(TItem item);

        protected virtual void DrawItemIcon(Rect rect, TItem item)
        {
        }

        protected virtual TaggedString GetTitleText()
        {
            return TitleKey.Translate();
        }

        protected virtual void BeforeDrawWindowContents()
        {
        }

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            BeforeDrawWindowContents();

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, TitleHeight), GetTitleText());
            Text.Font = GameFont.Small;

            float contentTop = inRect.y + 40f;
            if (ShowSearchRow)
            {
                Rect searchRect = new Rect(inRect.x, contentTop, inRect.width, SearchRowHeight);
                SearchBarWidget.DrawLabeledSearchRow(
                    searchRect,
                    SearchLabelKey,
                    SearchTooltipKey,
                    SearchControlName,
                    SearchLabelWidth,
                    ref searchText,
                    ref focusSearchOnNextDraw);
                contentTop = searchRect.yMax + 8f;
            }

            Rect listRect = new Rect(
                inRect.x,
                contentTop,
                inRect.width,
                inRect.yMax - contentTop);
            DrawList(listRect);
        }

        private void DrawList(Rect outRect)
        {
            string needle = NormalizeSearchText(searchText);

            tableRenderer.Draw(
                outRect,
                Options,
                item => MatchesSearch(item, needle),
                DrawRow,
                rect => Widgets.Label(rect, GetEmptyText(searchText)));
        }

        private void DrawRow(Rect rowRect, TItem item, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect buttonRect = new Rect(
                rowRect.xMax - SelectButtonWidth - 8f,
                rowRect.y + 8f,
                SelectButtonWidth,
                rowRect.height - 16f);

            Rect contentRect = new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - SelectButtonWidth - 24f, rowRect.height);
            Rect infoRect = new Rect(contentRect.x, rowRect.y + 6f, contentRect.width, rowRect.height - 12f);

            if (UseIconColumn)
            {
                Rect iconRect = new Rect(
                    rowRect.x + 8f,
                    rowRect.y + ((rowRect.height - IconSize) * 0.5f),
                    IconSize,
                    IconSize);
                DrawItemIcon(iconRect, item);

                infoRect = new Rect(
                    iconRect.xMax + 10f,
                    rowRect.y + 6f,
                    rowRect.width - IconSize - SelectButtonWidth - 34f,
                    rowRect.height - 12f);
            }

            DrawItemInfo(infoRect, item);

            if (Widgets.ButtonText(buttonRect, SelectButtonKey.Translate()))
            {
                OnItemSelected(item);
            }

            if (Widgets.ButtonInvisible(contentRect))
            {
                OnItemSelected(item);
            }
        }

        private static string NormalizeSearchText(string rawSearchText)
        {
            if (rawSearchText.NullOrEmpty())
            {
                return string.Empty;
            }

            return rawSearchText.Trim().ToLowerInvariant();
        }

        private TaggedString GetEmptyText(string currentSearchText)
        {
            if (ShowSearchRow)
            {
                return NoMatchesKey.Translate(currentSearchText);
            }

            return NoMatchesKey.Translate();
        }
    }
}
