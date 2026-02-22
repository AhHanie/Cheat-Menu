using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class SearchBarWidget
    {
        private const float Gap = 8f;
        private const float ClearButtonWidth = 84f;

        public static void DrawLabeledSearchRow(
            Rect rect,
            string labelKey,
            string tooltipKey,
            string controlName,
            float labelWidth,
            ref string searchText,
            ref bool focusOnNextDraw)
        {
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, labelKey.Translate());
            Text.Anchor = previousAnchor;

            float textWidth = rect.width - labelWidth - (Gap * 2f) - ClearButtonWidth;
            Rect textRect = new Rect(labelRect.xMax + Gap, rect.y, textWidth, rect.height);
            GUI.SetNextControlName(controlName);
            searchText = Widgets.TextField(textRect, searchText);
            TooltipHandler.TipRegion(textRect, tooltipKey.Translate());

            if (focusOnNextDraw)
            {
                GUI.FocusControl(controlName);
                focusOnNextDraw = false;
            }

            Rect clearButtonRect = new Rect(textRect.xMax + Gap, rect.y, ClearButtonWidth, rect.height);
            if (Widgets.ButtonText(clearButtonRect, "CheatMenu.Button.ClearSearch".Translate()))
            {
                searchText = string.Empty;
            }
        }
    }
}
