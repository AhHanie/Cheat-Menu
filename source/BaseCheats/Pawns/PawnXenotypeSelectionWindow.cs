using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class XenotypeSelectionOption
    {
        public XenotypeSelectionOption(XenotypeDef xenotypeDef, string displayLabel)
        {
            XenotypeDef = xenotypeDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public XenotypeDef XenotypeDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnXenotypeSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnSetXenotype.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float IconSize = 40f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<XenotypeSelectionOption> onXenotypeSelected;
        private readonly List<XenotypeSelectionOption> allOptions;
        private readonly SearchableTableRenderer<XenotypeSelectionOption> tableRenderer =
            new SearchableTableRenderer<XenotypeSelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnXenotypeSelectionWindow(Action<XenotypeSelectionOption> onXenotypeSelected)
        {
            this.onXenotypeSelected = onXenotypeSelected;
            allOptions = BuildXenotypeList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(920f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnSetXenotype.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnSetXenotype.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawXenotypeList(listRect);
        }

        private void DrawXenotypeList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearch,
                DrawXenotypeRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnSetXenotype.Window.NoMatches".Translate(searchText)));
        }

        private void DrawXenotypeRow(Rect rowRect, XenotypeSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - SelectButtonWidth - 34f, rowRect.height - 12f);

            DrawXenotypeIcon(iconRect, option);
            DrawXenotypeInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnSetXenotype.Window.SelectButton".Translate()))
            {
                SelectXenotype(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectXenotype(option);
            }
        }

        private static void DrawXenotypeInfo(Rect rect, XenotypeSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetXenotype.Window.InfoLine".Translate(option.XenotypeDef.defName));
            Text.Font = GameFont.Small;
        }

        private static void DrawXenotypeIcon(Rect iconRect, XenotypeSelectionOption option)
        {
            Texture2D icon = option.XenotypeDef?.Icon ?? BaseContent.BadTex;
            if (icon == null)
            {
                icon = BaseContent.BadTex;
            }

            //Color previousColor = GUI.color;
            //GUI.color = option?.XenotypeDef?.IconColor ?? Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            //GUI.color = previousColor;
        }

        private bool MatchesSearch(XenotypeSelectionOption option)
        {
            XenotypeDef xenotypeDef = option.XenotypeDef;
            if (xenotypeDef == null)
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
            string xenotypeLabel = (xenotypeDef.label ?? string.Empty).ToLowerInvariant();
            string defName = xenotypeDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || xenotypeLabel.Contains(needle) || defName.Contains(needle);
        }

        private void SelectXenotype(XenotypeSelectionOption option)
        {
            Close();
            onXenotypeSelected?.Invoke(option);
        }

        private static List<XenotypeSelectionOption> BuildXenotypeList()
        {
            return DefDatabase<XenotypeDef>.AllDefsListForReading
                .Where(xenotypeDef => xenotypeDef != null)
                .Select(xenotypeDef => new XenotypeSelectionOption(
                    xenotypeDef,
                    xenotypeDef.defName))
                .OrderBy(option => option.XenotypeDef.defName)
                .ToList();
        }
    }
}
