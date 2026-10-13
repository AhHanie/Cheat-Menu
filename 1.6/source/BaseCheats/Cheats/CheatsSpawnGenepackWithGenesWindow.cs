using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class CheatsSpawnGenepackWithGenesWindow : Window
    {
        private const string SearchControlName = "CheatMenu.Cheats.SpawnGenepackWithGenes.SearchField";
        private const float HeaderHeight = 36f;
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 62f;
        private const float RowSpacing = 4f;
        private const float PanelHeaderHeight = 32f;
        private const float PanelPadding = 12f;
        private const float SummaryCountsHeight = 24f;
        private const float SummaryInnerSpacing = 8f;
        private const float MinimumSummaryBodyHeight = 72f;
        private const float ButtonWidth = 96f;
        private const float ClearButtonWidth = 112f;
        private const float BottomButtonWidth = 140f;
        private const float BottomButtonHeight = 36f;
        private const float IconSize = 32f;

        private readonly Action<List<GeneDef>> onConfirm;
        private readonly List<GeneDef> allGenes;
        private readonly List<GeneDef> selectedGenes = new List<GeneDef>();
        private readonly SearchableTableRenderer<GeneDef> selectedGenesRenderer =
            new SearchableTableRenderer<GeneDef>(RowHeight, RowSpacing);
        private readonly SearchableTableRenderer<GeneDef> allGenesRenderer =
            new SearchableTableRenderer<GeneDef>(RowHeight, RowSpacing);

        private readonly List<GeneDef> effectiveGenes = new List<GeneDef>();
        private readonly HashSet<GeneDef> effectiveGenesSet = new HashSet<GeneDef>();
        private readonly List<GeneDef> missingPrerequisiteGenes = new List<GeneDef>();

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;
        private int totalComplexity;
        private int totalMetabolism;
        private int totalArchites;

        public CheatsSpawnGenepackWithGenesWindow(Action<List<GeneDef>> onConfirm)
        {
            this.onConfirm = onConfirm;
            allGenes = DefDatabase<GeneDef>.AllDefsListForReading
                .OrderBy(GetGeneLabel)
                .ThenBy(gene => gene.defName)
                .ToList();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(1420f, 860f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
            RecalculateSelection();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight), "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.Title".Translate());
            Text.Font = GameFont.Small;

            float summaryTop = inRect.y + HeaderHeight + 8f;
            float summaryBodyHeight = Mathf.Max(MinimumSummaryBodyHeight, BiostatsTable.HeightForBiostats(totalArchites));
            float summaryHeight = (PanelPadding * 2f) + SummaryCountsHeight + SummaryInnerSpacing + summaryBodyHeight;
            Rect summaryRect = new Rect(inRect.x, summaryTop, inRect.width, summaryHeight);
            DrawSummary(summaryRect);

            float contentTop = summaryRect.yMax + 12f;
            Rect contentRect = new Rect(inRect.x, contentTop, inRect.width, inRect.height - (contentTop - inRect.y) - BottomButtonHeight - 12f);
            DrawContent(contentRect);

            Rect bottomRect = new Rect(inRect.x, contentRect.yMax + 12f, inRect.width, BottomButtonHeight);
            DrawBottomButtons(bottomRect);
        }

        private void DrawSummary(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Rect countsRect = new Rect(rect.x + PanelPadding, rect.y + PanelPadding, rect.width - (PanelPadding * 2f), SummaryCountsHeight);
            Widgets.Label(
                countsRect,
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.SelectionSummary".Translate(
                    selectedGenes.Count,
                    effectiveGenes.Count,
                    missingPrerequisiteGenes.Count));

            Rect biostatsRect = new Rect(
                rect.x + PanelPadding,
                countsRect.yMax + SummaryInnerSpacing,
                420f,
                rect.height - (PanelPadding * 2f) - countsRect.height - SummaryInnerSpacing);
            BiostatsTable.Draw(
                biostatsRect,
                totalComplexity,
                totalMetabolism,
                totalArchites,
                drawMax: true,
                ignoreLimits: false);

            Rect statusRect = new Rect(
                biostatsRect.xMax + 20f,
                countsRect.yMax + SummaryInnerSpacing,
                rect.xMax - biostatsRect.xMax - 32f,
                rect.height - (PanelPadding * 2f) - countsRect.height - SummaryInnerSpacing);
            DrawSummaryStatus(statusRect);
        }

        private void DrawSummaryStatus(Rect rect)
        {
            float lineHeight = 24f;
            Rect currentRect = new Rect(rect.x, rect.y, rect.width, lineHeight);

            if (selectedGenes.Count == 0)
            {
                GUI.color = ColorLibrary.RedReadable;
                Widgets.Label(currentRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.NoGenesSelected".Translate());
                GUI.color = Color.white;
                return;
            }

            Widgets.Label(currentRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.ReadyHint".Translate());
            currentRect.y += lineHeight + 4f;

            if (missingPrerequisiteGenes.Count > 0)
            {
                GUI.color = ColorLibrary.RedReadable;
                Widgets.Label(
                    currentRect,
                    "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.MissingPrerequisites".Translate(missingPrerequisiteGenes.Count));
                GUI.color = Color.white;
                currentRect.y += lineHeight + 2f;
            }

            int overriddenCount = selectedGenes.Count - effectiveGenes.Count;
            if (overriddenCount > 0)
            {
                GUI.color = Color.yellow;
                Widgets.Label(
                    currentRect,
                    "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.OverriddenGenes".Translate(overriddenCount));
                GUI.color = Color.white;
            }
        }

        private void DrawContent(Rect rect)
        {
            float gap = 12f;
            float panelWidth = (rect.width - gap) * 0.5f;
            Rect selectedRect = new Rect(rect.x, rect.y, panelWidth, rect.height);
            Rect availableRect = new Rect(selectedRect.xMax + gap, rect.y, rect.width - panelWidth - gap, rect.height);

            DrawSelectedGenesPanel(selectedRect);
            DrawAvailableGenesPanel(availableRect);
        }

        private void DrawSelectedGenesPanel(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Rect headerRect = new Rect(rect.x + PanelPadding, rect.y + PanelPadding, rect.width - (PanelPadding * 2f), PanelHeaderHeight);
            Widgets.Label(
                new Rect(headerRect.x, headerRect.y, headerRect.width - ClearButtonWidth - 8f, headerRect.height),
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.SelectedPanel".Translate(selectedGenes.Count));

            if (Widgets.ButtonText(
                new Rect(headerRect.xMax - ClearButtonWidth, headerRect.y, ClearButtonWidth, headerRect.height),
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.ClearAll".Translate()))
            {
                selectedGenes.Clear();
                RecalculateSelection();
            }

            Rect listRect = new Rect(
                rect.x + PanelPadding,
                headerRect.yMax + 8f,
                rect.width - (PanelPadding * 2f),
                rect.height - headerRect.height - (PanelPadding * 2f) - 8f);

            selectedGenesRenderer.Draw(
                listRect,
                selectedGenes,
                gene => true,
                DrawSelectedGeneRow,
                emptyRect => Widgets.Label(emptyRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.NoSelectedGenes".Translate()));
        }

        private void DrawAvailableGenesPanel(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Rect headerRect = new Rect(rect.x + PanelPadding, rect.y + PanelPadding, rect.width - (PanelPadding * 2f), PanelHeaderHeight);
            Widgets.Label(headerRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.AvailablePanel".Translate());

            Rect searchRect = new Rect(rect.x + PanelPadding, headerRect.yMax + 8f, rect.width - (PanelPadding * 2f), SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                rect.x + PanelPadding,
                searchRect.yMax + 8f,
                rect.width - (PanelPadding * 2f),
                rect.height - (searchRect.yMax - rect.y) - PanelPadding - 8f);

            string needle = searchText.NullOrEmpty() ? string.Empty : searchText.Trim().ToLowerInvariant();
            allGenesRenderer.Draw(
                listRect,
                allGenes,
                gene => MatchesSearch(gene, needle),
                DrawAvailableGeneRow,
                emptyRect => Widgets.Label(emptyRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.NoMatches".Translate(searchText)));
        }

        private void DrawSelectedGeneRow(Rect rowRect, GeneDef gene, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);
            TooltipHandler.TipRegion(rowRect, BuildTooltip(gene));

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            Rect buttonRect = new Rect(rowRect.xMax - ButtonWidth - 8f, rowRect.y + 10f, ButtonWidth, rowRect.height - 20f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - ButtonWidth - 34f, rowRect.height - 12f);

            DrawGeneIcon(iconRect, gene);
            DrawSelectedGeneInfo(infoRect, gene);

            if (Widgets.ButtonText(buttonRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.RemoveButton".Translate()))
            {
                ToggleGene(gene);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                ToggleGene(gene);
            }
        }

        private void DrawAvailableGeneRow(Rect rowRect, GeneDef gene, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            if (selectedGenes.Contains(gene))
            {
                Widgets.DrawLightHighlight(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);
            TooltipHandler.TipRegion(rowRect, BuildTooltip(gene));

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            Rect buttonRect = new Rect(rowRect.xMax - ButtonWidth - 8f, rowRect.y + 10f, ButtonWidth, rowRect.height - 20f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - ButtonWidth - 34f, rowRect.height - 12f);

            DrawGeneIcon(iconRect, gene);
            DrawAvailableGeneInfo(infoRect, gene);

            string buttonLabelKey = selectedGenes.Contains(gene)
                ? "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.RemoveButton"
                : "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.AddButton";
            if (Widgets.ButtonText(buttonRect, buttonLabelKey.Translate()))
            {
                ToggleGene(gene);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                ToggleGene(gene);
            }
        }

        private void DrawSelectedGeneInfo(Rect rect, GeneDef gene)
        {
            Color previousColor = GUI.color;
            if (selectedGenes.Contains(gene) && gene.prerequisite != null && !selectedGenes.Contains(gene.prerequisite))
            {
                GUI.color = ColorLibrary.RedReadable;
            }
            else if (!effectiveGenesSet.Contains(gene))
            {
                GUI.color = Color.gray;
            }

            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), GetGeneLabel(gene));
            GUI.color = previousColor;

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.y + 22f, rect.width, 18f),
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.GeneInfoLine".Translate(
                    gene.defName,
                    gene.biostatCpx,
                    gene.biostatMet.ToStringWithSign(),
                    gene.biostatArc));

            GUI.color = GetStatusColor(gene);
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 18f, rect.width, 18f),
                GetStatusText(gene));
            GUI.color = previousColor;
            Text.Font = GameFont.Small;
        }

        private void DrawAvailableGeneInfo(Rect rect, GeneDef gene)
        {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), GetGeneLabel(gene));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.GeneInfoLine".Translate(
                    gene.defName,
                    gene.biostatCpx,
                    gene.biostatMet.ToStringWithSign(),
                    gene.biostatArc));
            Text.Font = GameFont.Small;
        }

        private void DrawBottomButtons(Rect rect)
        {
            Rect cancelRect = new Rect(rect.x, rect.y, BottomButtonWidth, rect.height);
            if (Widgets.ButtonText(cancelRect, "CheatMenu.Button.Cancel".Translate()))
            {
                Close();
            }

            bool previousEnabled = GUI.enabled;
            GUI.enabled = selectedGenes.Count > 0;
            Rect spawnRect = new Rect(rect.xMax - BottomButtonWidth, rect.y, BottomButtonWidth, rect.height);
            if (Widgets.ButtonText(spawnRect, "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.SpawnButton".Translate()))
            {
                List<GeneDef> genesToSpawn = new List<GeneDef>(selectedGenes);
                Close();
                onConfirm?.Invoke(genesToSpawn);
            }

            GUI.enabled = previousEnabled;
        }

        private void ToggleGene(GeneDef gene)
        {
            if (!selectedGenes.Remove(gene))
            {
                selectedGenes.Add(gene);
            }

            RecalculateSelection();
        }

        private void RecalculateSelection()
        {
            effectiveGenes.Clear();
            effectiveGenesSet.Clear();
            missingPrerequisiteGenes.Clear();
            totalComplexity = 0;
            totalMetabolism = 0;
            totalArchites = 0;

            if (selectedGenes.Count > 0)
            {
                effectiveGenes.AddRange(selectedGenes.NonOverriddenGenes(xenogene: true).Distinct());
                for (int i = 0; i < effectiveGenes.Count; i++)
                {
                    effectiveGenesSet.Add(effectiveGenes[i]);
                }
            }

            for (int i = 0; i < selectedGenes.Count; i++)
            {
                GeneDef selectedGene = selectedGenes[i];
                if (selectedGene.prerequisite != null && !selectedGenes.Contains(selectedGene.prerequisite))
                {
                    missingPrerequisiteGenes.Add(selectedGene);
                }
            }

            if (effectiveGenes.Count == 0)
            {
                return;
            }

            totalComplexity = effectiveGenes.Sum(gene => gene.biostatCpx);
            totalMetabolism = effectiveGenes.Sum(gene => gene.biostatMet);
            totalArchites = effectiveGenes.Sum(gene => gene.biostatArc);
        }

        private static bool MatchesSearch(GeneDef gene, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetGeneLabel(gene).ToLowerInvariant();
            string defName = gene.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        private string GetStatusText(GeneDef gene)
        {
            if (gene.prerequisite != null && !selectedGenes.Contains(gene.prerequisite))
            {
                return "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.StatusMissingPrerequisite".Translate(gene.prerequisite.LabelCap);
            }

            if (!effectiveGenesSet.Contains(gene))
            {
                return "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.StatusOverridden".Translate();
            }

            return "CheatMenu.Cheats.SpawnGenepackWithGenes.Window.StatusActive".Translate();
        }

        private Color GetStatusColor(GeneDef gene)
        {
            if (gene.prerequisite != null && !selectedGenes.Contains(gene.prerequisite))
            {
                return ColorLibrary.RedReadable;
            }

            if (!effectiveGenesSet.Contains(gene))
            {
                return Color.gray;
            }

            return Color.white;
        }

        private static void DrawGeneIcon(Rect rect, GeneDef gene)
        {
            Color previousColor = GUI.color;
            GUI.color = gene.IconColor;
            GUI.DrawTexture(rect, gene.Icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private string BuildTooltip(GeneDef gene)
        {
            string tooltip = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.DescriptionFull;
            if (selectedGenes.Contains(gene))
            {
                tooltip += "\n\n" + GetStatusText(gene);
            }

            tooltip += "\n\n" + (selectedGenes.Contains(gene) ? "ClickToRemove" : "ClickToAdd").Translate().Colorize(ColoredText.SubtleGrayColor);
            return tooltip;
        }

        private static string GetGeneLabel(GeneDef gene)
        {
            return (gene.label ?? gene.defName).CapitalizeFirst();
        }
    }
}
