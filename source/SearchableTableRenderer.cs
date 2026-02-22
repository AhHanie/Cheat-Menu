using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Reusable searchable table renderer with virtualized row drawing.
    /// Windows provide filtering logic and custom row rendering.
    /// </summary>
    public sealed class SearchableTableRenderer<TItem>
    {
        private readonly float rowHeight;
        private readonly float rowSpacing;

        private Vector2 scrollPosition;

        public SearchableTableRenderer(float rowHeight, float rowSpacing)
        {
            this.rowHeight = rowHeight;
            this.rowSpacing = rowSpacing;
        }

        public void Draw(
            Rect outRect,
            IReadOnlyList<TItem> sourceItems,
            Func<TItem, bool> matchesSearch,
            Action<Rect, TItem, bool> drawRow,
            Action<Rect> drawEmpty)
        {
            if (sourceItems == null || sourceItems.Count == 0)
            {
                drawEmpty?.Invoke(outRect);
                return;
            }

            List<TItem> filtered = sourceItems.Where(matchesSearch).ToList();
            if (filtered.Count == 0)
            {
                drawEmpty?.Invoke(outRect);
                return;
            }

            float viewHeight = filtered.Count * (rowHeight + rowSpacing);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            DrawVisibleRows(viewRect, outRect.height, filtered, drawRow);
            Widgets.EndScrollView();
        }

        private void DrawVisibleRows(Rect viewRect, float visibleHeight, List<TItem> filtered, Action<Rect, TItem, bool> drawRow)
        {
            float y = 0f;
            float visibleTop = scrollPosition.y;
            float visibleBottom = scrollPosition.y + visibleHeight;

            for (int i = 0; i < filtered.Count; i++)
            {
                TItem item = filtered[i];
                if (y + rowHeight >= visibleTop && y <= visibleBottom)
                {
                    Rect rowRect = new Rect(viewRect.x, y, viewRect.width, rowHeight);
                    drawRow(rowRect, item, i % 2 == 0);
                }

                y += rowHeight + rowSpacing;
            }
        }
    }
}
