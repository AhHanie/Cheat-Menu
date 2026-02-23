using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnSkillSelectionOption
    {
        public PawnSkillSelectionOption(SkillDef skillDef, string displayLabel)
        {
            SkillDef = skillDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public SkillDef SkillDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnSkillSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnSetSkill.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<PawnSkillSelectionOption> onSkillSelected;
        private readonly List<PawnSkillSelectionOption> allOptions;
        private readonly SearchableTableRenderer<PawnSkillSelectionOption> tableRenderer =
            new SearchableTableRenderer<PawnSkillSelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnSkillSelectionWindow(Action<PawnSkillSelectionOption> onSkillSelected)
        {
            this.onSkillSelected = onSkillSelected;
            allOptions = BuildSkillList();

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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnSetSkill.SkillWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnSetSkill.SkillWindow.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawSkillList(listRect);
        }

        private void DrawSkillList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearch,
                DrawSkillRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnSetSkill.SkillWindow.NoMatches".Translate(searchText)));
        }

        private void DrawSkillRow(Rect rowRect, PawnSkillSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y + 6f, rowRect.width - SelectButtonWidth - 24f, rowRect.height - 12f);

            DrawSkillInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnSetSkill.SkillWindow.SelectButton".Translate()))
            {
                SelectSkill(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectSkill(option);
            }
        }

        private static void DrawSkillInfo(Rect rect, PawnSkillSelectionOption option)
        {
            SkillDef skillDef = option?.SkillDef;
            if (skillDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetSkill.SkillWindow.InfoLine".Translate(skillDef.defName));
            Text.Font = GameFont.Small;
        }

        private bool MatchesSearch(PawnSkillSelectionOption option)
        {
            SkillDef skillDef = option?.SkillDef;
            if (skillDef == null)
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
            string skillLabel = (skillDef.label ?? string.Empty).ToLowerInvariant();
            string defName = skillDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || skillLabel.Contains(needle) || defName.Contains(needle);
        }

        private void SelectSkill(PawnSkillSelectionOption option)
        {
            Close();
            onSkillSelected?.Invoke(option);
        }

        private static List<PawnSkillSelectionOption> BuildSkillList()
        {
            List<PawnSkillSelectionOption> result = new List<PawnSkillSelectionOption>();
            foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                if (skillDef == null)
                {
                    continue;
                }

                string label = skillDef.label.NullOrEmpty() ? skillDef.defName : skillDef.label.CapitalizeFirst();
                result.Add(new PawnSkillSelectionOption(skillDef, label));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.SkillDef.defName)
                .ToList();
        }
    }
}
