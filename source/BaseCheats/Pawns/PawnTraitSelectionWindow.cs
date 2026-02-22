using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class TraitSelection
    {
        public TraitSelection(TraitDef traitDef, int degree, string displayLabel)
        {
            TraitDef = traitDef;
            Degree = degree;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public TraitDef TraitDef { get; }

        public int Degree { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnTraitSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnGiveTrait.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<TraitSelection> onTraitSelected;
        private readonly List<TraitSelection> allTraits;
        private readonly SearchableTableRenderer<TraitSelection> tableRenderer =
            new SearchableTableRenderer<TraitSelection>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnTraitSelectionWindow(Action<TraitSelection> onTraitSelected)
        {
            this.onTraitSelected = onTraitSelected;
            allTraits = BuildTraitList();

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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnGiveTrait.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnGiveTrait.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawTraitList(listRect);
        }

        private void DrawTraitList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allTraits,
                MatchesSearch,
                DrawTraitRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnGiveTrait.Window.NoMatches".Translate(searchText)));
        }

        private void DrawTraitRow(Rect rowRect, TraitSelection selection, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - SelectButtonWidth - 24f, rowRect.height - 12f);

            DrawTraitInfo(infoRect, selection);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnGiveTrait.Window.SelectButton".Translate()))
            {
                SelectTrait(selection);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectTrait(selection);
            }
        }

        private static void DrawTraitInfo(Rect rect, TraitSelection selection)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), selection.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnGiveTrait.Window.InfoLine".Translate(selection.TraitDef.defName, selection.Degree));
            Text.Font = GameFont.Small;
        }

        private bool MatchesSearch(TraitSelection selection)
        {
            if (selection == null || selection.TraitDef == null)
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

            string displayLabel = (selection.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string traitLabel = (selection.TraitDef.label ?? string.Empty).ToLowerInvariant();
            string defName = selection.TraitDef.defName.ToLowerInvariant();
            string degreeString = selection.Degree.ToString();

            return displayLabel.Contains(needle)
                || traitLabel.Contains(needle)
                || defName.Contains(needle)
                || degreeString.Contains(needle);
        }

        private void SelectTrait(TraitSelection selection)
        {
            Close();
            onTraitSelected?.Invoke(selection);
        }

        private static List<TraitSelection> BuildTraitList()
        {
            List<TraitSelection> result = new List<TraitSelection>();
            foreach (TraitDef traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                if (traitDef?.degreeDatas == null)
                {
                    continue;
                }

                for (int j = 0; j < traitDef.degreeDatas.Count; j++)
                {
                    TraitDegreeData degreeData = traitDef.degreeDatas[j];
                    if (degreeData == null)
                    {
                        continue;
                    }

                    string degreeLabel = degreeData.label ?? traitDef.label ?? traitDef.defName;
                    string displayLabel = degreeLabel + " (" + degreeData.degree + ")";
                    result.Add(new TraitSelection(traitDef, degreeData.degree, displayLabel));
                }
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.TraitDef.defName)
                .ToList();
        }
    }
}
