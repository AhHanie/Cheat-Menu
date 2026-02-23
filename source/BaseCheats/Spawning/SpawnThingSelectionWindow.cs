using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class SpawnThingSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.SpawnThing.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float IconSize = 40f;
        private const float SelectButtonWidth = 86f;

        private readonly Action<ThingDef> onThingSelected;
        private readonly List<ThingDef> allThingDefs;
        private readonly SearchableTableRenderer<ThingDef> tableRenderer =
            new SearchableTableRenderer<ThingDef>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public SpawnThingSelectionWindow(Action<ThingDef> onThingSelected)
        {
            this.onThingSelected = onThingSelected;
            allThingDefs = BuildThingDefList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(980f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.SpawnThing.SelectWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            DrawSearchRow(searchRect);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawThingList(listRect);
        }

        private void DrawSearchRow(Rect rect)
        {
            SearchBarWidget.DrawLabeledSearchRow(
                rect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.SpawnThing.SelectWindow.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);
        }

        private void DrawThingList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allThingDefs,
                MatchesSearch,
                DrawThingRow,
                rect => Widgets.Label(rect, "CheatMenu.SpawnThing.SelectWindow.NoMatches".Translate(searchText)));
        }

        private void DrawThingRow(Rect rowRect, ThingDef thingDef, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            DrawThingIcon(iconRect, thingDef);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - SelectButtonWidth - 34f, rowRect.height - 12f);

            DrawThingInfo(infoRect, thingDef);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.SpawnThing.SelectWindow.SelectButton".Translate()))
            {
                SelectThing(thingDef);
            }

            if (Widgets.ButtonInvisible(rowRect))
            {
                SelectThing(thingDef);
            }
        }

        private static void DrawThingInfo(Rect rect, ThingDef thingDef)
        {
            string label = GetSafeLabel(thingDef);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.SpawnThing.SelectWindow.DefNameLine".Translate(thingDef.defName));
            Text.Font = GameFont.Small;
        }

        private static void DrawThingIcon(Rect iconRect, ThingDef thingDef)
        {
            Texture2D icon = GetIcon(thingDef);
            if (icon == null)
            {
                Widgets.DrawBoxSolid(iconRect, new Color(0.2f, 0.2f, 0.2f));
                Widgets.Label(iconRect, "?");
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = thingDef?.uiIconColor ?? Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private static Texture2D GetIcon(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return BaseContent.BadTex;
            }

            if (thingDef.uiIcon != null)
            {
                return thingDef.uiIcon;
            }

            return BaseContent.BadTex;
        }

        private bool MatchesSearch(ThingDef thingDef)
        {
            if (thingDef == null)
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

            string label = GetSafeLabel(thingDef).ToLowerInvariant();
            string defName = thingDef.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        private void SelectThing(ThingDef thingDef)
        {
            Close();
            onThingSelected?.Invoke(thingDef);
        }

        private static List<ThingDef> BuildThingDefList()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(IsSelectableThingDef)
                .OrderBy(GetSafeLabel)
                .ThenBy((ThingDef thingDef) => thingDef.defName)
                .ToList();
        }

        private static bool IsSelectableThingDef(ThingDef thingDef)
        {
            // Match RimWorld's debug spawn item filtering behavior.
            return DebugThingPlaceHelper.IsDebugSpawnable(thingDef);
        }

        private static string GetSafeLabel(ThingDef thingDef)
        {
            return thingDef?.label ?? string.Empty;
        }
    }
}
