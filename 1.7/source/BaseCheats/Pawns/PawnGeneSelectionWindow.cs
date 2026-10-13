using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public enum GeneSelectionMode
    {
        Xenogene,
        Endogene,
        Heritable
    }

    public class GeneSelectionOption
    {
        public GeneSelectionOption(GeneDef geneDef, GeneSelectionMode mode)
        {
            GeneDef = geneDef;
            Mode = mode;
        }

        public GeneDef GeneDef { get; }

        public GeneSelectionMode Mode { get; }

        public string DisplayLabel { get; }

        public bool IsXenogene => Mode == GeneSelectionMode.Xenogene;
    }

    public class PawnGeneSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnAddGene.SearchField";
        private const float SearchRowHeight = 34f;
        private const float ModeButtonsRowHeight = 32f;
        private const float ModeButtonWidth = 136f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float IconSize = 40f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<GeneSelectionOption> onGeneSelected;
        private readonly List<GeneSelectionOption> allOptions;
        private readonly SearchableTableRenderer<GeneSelectionOption> tableRenderer =
            new SearchableTableRenderer<GeneSelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;
        private GeneSelectionMode selectedMode = GeneSelectionMode.Xenogene;

        public PawnGeneSelectionWindow(Action<GeneSelectionOption> onGeneSelected)
        {
            this.onGeneSelected = onGeneSelected;
            allOptions = BuildGeneOptions();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(920f, 720f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnAddGene.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect modeRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, ModeButtonsRowHeight);
            DrawModeButtons(modeRect);

            Rect searchRect = new Rect(inRect.x, modeRect.yMax + 8f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnAddGene.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawGeneList(listRect);
        }

        private void DrawModeButtons(Rect rect)
        {
            Rect xenogeneRect = new Rect(rect.x, rect.y, ModeButtonWidth, rect.height);
            Rect endogeneRect = new Rect(xenogeneRect.xMax + 8f, rect.y, ModeButtonWidth, rect.height);
            Rect heritableRect = new Rect(endogeneRect.xMax + 8f, rect.y, ModeButtonWidth, rect.height);

            DrawModeButton(xenogeneRect, GeneSelectionMode.Xenogene, "CheatMenu.PawnAddGene.Mode.Xenogene");
            DrawModeButton(endogeneRect, GeneSelectionMode.Endogene, "CheatMenu.PawnAddGene.Mode.Endogene");
            DrawModeButton(heritableRect, GeneSelectionMode.Heritable, "CheatMenu.PawnAddGene.Mode.Heritable");
        }

        private void DrawModeButton(Rect rect, GeneSelectionMode mode, string labelKey)
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = selectedMode != mode;
            if (Widgets.ButtonText(rect, labelKey.Translate()))
            {
                selectedMode = mode;
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawGeneList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearchAndMode,
                DrawGeneRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnAddGene.Window.NoMatches".Translate(searchText)));
        }

        private void DrawGeneRow(Rect rowRect, GeneSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - SelectButtonWidth - 34f, rowRect.height - 12f);

            DrawGeneIcon(iconRect, option);
            DrawGeneInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnAddGene.Window.SelectButton".Translate()))
            {
                SelectGene(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectGene(option);
            }
        }

        private static void DrawGeneInfo(Rect rect, GeneSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnAddGene.Window.InfoLine".Translate(option.GeneDef.defName));
            Text.Font = GameFont.Small;
        }

        private static void DrawGeneIcon(Rect iconRect, GeneSelectionOption option)
        {
            Texture2D icon = option.GeneDef.Icon;
            Color previousColor = GUI.color;
            GUI.color = option?.GeneDef?.IconColor ?? Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private bool MatchesSearchAndMode(GeneSelectionOption option)
        {
            if (option.Mode != selectedMode)
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

            string geneLabel = option.GeneDef.label.ToLowerInvariant();
            string defName = option.GeneDef.defName.ToLowerInvariant();

            return geneLabel.Contains(needle)
                || defName.Contains(needle);
        }

        private void SelectGene(GeneSelectionOption option)
        {
            Close();
            onGeneSelected?.Invoke(option);
        }

        private static List<GeneSelectionOption> BuildGeneOptions()
        {
            List<GeneSelectionOption> result = new List<GeneSelectionOption>();
            List<XenotypeDef> allXenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading.ToList();

            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefsListForReading.OrderBy(def => def.defName))
            {
                // Xenogene list: all genes
                result.Add(new GeneSelectionOption(geneDef, GeneSelectionMode.Xenogene));

                // Endogene list: non-archite genes only
                if (geneDef.biostatArc <= 0)
                {
                    result.Add(new GeneSelectionOption(geneDef, GeneSelectionMode.Endogene));
                }

                // Heritable list: non-archite + heritable-capable genes
                if (geneDef.biostatArc <= 0 && IsHeritableGene(geneDef, allXenotypes))
                {
                    result.Add(new GeneSelectionOption(geneDef, GeneSelectionMode.Heritable));
                }
            }

            return result;
        }

        private static bool IsHeritableGene(GeneDef geneDef, List<XenotypeDef> allXenotypes)
        {
            return geneDef.endogeneCategory != EndogeneCategory.None
                || !geneDef.forcedTraits.NullOrEmpty()
                || allXenotypes.Any(xenotype => xenotype.inheritable && xenotype.genes.Contains(geneDef));
        }
    }
}
