using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class AbilitySelectionOption
    {
        public AbilitySelectionOption(bool isAll, AbilityDef abilityDef, string displayLabel)
        {
            IsAll = isAll;
            AbilityDef = abilityDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public bool IsAll { get; }

        public AbilityDef AbilityDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnAbilitySelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.PawnGiveAbility.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float IconSize = 40f;
        private const float SelectButtonWidth = 92f;

        private readonly Action<AbilitySelectionOption> onAbilitySelected;
        private readonly List<AbilitySelectionOption> allOptions;
        private readonly SearchableTableRenderer<AbilitySelectionOption> tableRenderer =
            new SearchableTableRenderer<AbilitySelectionOption>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public PawnAbilitySelectionWindow(Action<AbilitySelectionOption> onAbilitySelected)
        {
            this.onAbilitySelected = onAbilitySelected;
            allOptions = BuildAbilityList();

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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnGiveAbility.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            SearchBarWidget.DrawLabeledSearchRow(
                searchRect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.PawnGiveAbility.Window.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawAbilityList(listRect);
        }

        private void DrawAbilityList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allOptions,
                MatchesSearch,
                DrawAbilityRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnGiveAbility.Window.NoMatches".Translate(searchText)));
        }

        private void DrawAbilityRow(Rect rowRect, AbilitySelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - SelectButtonWidth - 34f, rowRect.height - 12f);

            DrawAbilityIcon(iconRect, option);
            DrawAbilityInfo(infoRect, option);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnGiveAbility.Window.SelectButton".Translate()))
            {
                SelectAbility(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectAbility(option);
            }
        }

        private static void DrawAbilityInfo(Rect rect, AbilitySelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            string defName = option.IsAll
                ? "CheatMenu.PawnGiveAbility.Window.AllDefName".Translate().ToString()
                : (option.AbilityDef?.defName ?? string.Empty);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnGiveAbility.Window.InfoLine".Translate(defName));
            Text.Font = GameFont.Small;
        }

        private static void DrawAbilityIcon(Rect iconRect, AbilitySelectionOption option)
        {
            if (option != null && option.IsAll)
            {
                Widgets.DrawBoxSolid(iconRect, new Color(0.23f, 0.23f, 0.23f));
                TextAnchor previousAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(iconRect, "*");
                Text.Anchor = previousAnchor;
                return;
            }

            Texture2D icon = option?.AbilityDef?.uiIcon ?? BaseContent.BadTex;
            if (icon == null)
            {
                icon = BaseContent.BadTex;
            }

            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private bool MatchesSearch(AbilitySelectionOption option)
        {
            if (option == null)
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
            string abilityLabel = (option.AbilityDef?.label ?? string.Empty).ToLowerInvariant();
            string defName = (option.AbilityDef?.defName ?? string.Empty).ToLowerInvariant();

            if (option.IsAll)
            {
                string allAlias = "all";
                return displayLabel.Contains(needle) || allAlias.Contains(needle);
            }

            return displayLabel.Contains(needle) || abilityLabel.Contains(needle) || defName.Contains(needle);
        }

        private void SelectAbility(AbilitySelectionOption option)
        {
            Close();
            onAbilitySelected?.Invoke(option);
        }

        private static List<AbilitySelectionOption> BuildAbilityList()
        {
            List<AbilitySelectionOption> result = new List<AbilitySelectionOption>
            {
                new AbilitySelectionOption(
                    isAll: true,
                    abilityDef: null,
                    displayLabel: "CheatMenu.PawnGiveAbility.Window.AllOption".Translate())
            };

            List<AbilitySelectionOption> abilityOptions = new List<AbilitySelectionOption>();
            foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
            {
                if (abilityDef == null)
                {
                    continue;
                }

                string label = abilityDef.label ?? abilityDef.defName ?? string.Empty;
                abilityOptions.Add(new AbilitySelectionOption(false, abilityDef, label));
            }

            result.AddRange(
                abilityOptions
                    .OrderBy(option => option.DisplayLabel)
                    .ThenBy(option => option.AbilityDef.defName));

            return result;
        }
    }
}
