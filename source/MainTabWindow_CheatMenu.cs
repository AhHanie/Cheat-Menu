using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class MainTabWindow_CheatMenu : MainTabWindow
    {
        private const string SearchControlName = "CheatMenu.Main.SearchField";
        private const float SearchRowHeight = 34f;
        private const float CategoryHeaderHeight = 30f;
        private const float CheatCardHeight = 88f;
        private const float RowSpacing = 6f;
        private const float SidePadding = 10f;
        private const float ExecuteButtonWidth = 172f;
        private const float ExecuteButtonHeight = 36f;

        private Vector2 scrollPosition;
        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public override Vector2 RequestedTabSize => new Vector2(1024f, 720f);

        public override void PreOpen()
        {
            base.PreOpen();
            if (ModSettings.ClearCachedSearchOnMenuReopen)
            {
                searchText = string.Empty;
                scrollPosition = Vector2.zero;
            }

            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 36f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "CheatMenu.Window.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, titleRect.yMax + 4f, inRect.width, SearchRowHeight);
            DrawSearchRow(searchRect);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawCheatList(listRect);
        }

        private void DrawSearchRow(Rect rect)
        {
            SearchBarWidget.DrawLabeledSearchRow(
                rect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.Window.SearchTooltip",
                SearchControlName,
                130f,
                ref searchText,
                ref focusSearchOnNextDraw);
        }

        private void DrawCheatList(Rect outRect)
        {
            IReadOnlyList<CheatDefinition> allCheats = CheatRegistry.GetAllCheats();
            if (allCheats.Count == 0)
            {
                Widgets.Label(outRect, "CheatMenu.Window.NoCheatsRegistered".Translate());
                return;
            }

            List<CheatDefinition> visibleCheats = allCheats
                .Where(cheat => cheat.IsVisibleNow())
                .ToList();

            if (visibleCheats.Count == 0)
            {
                Widgets.Label(outRect, "CheatMenu.Window.NoVisibleCheats".Translate());
                return;
            }

            List<CheatDefinition> filteredCheats = visibleCheats
                .Where(MatchesSearch)
                .ToList();

            if (filteredCheats.Count == 0)
            {
                Widgets.Label(outRect, "CheatMenu.Window.NoCheatsMatchingSearch".Translate(searchText));
                return;
            }

            List<CategoryGroup> groups = filteredCheats
                .GroupBy(cheat => cheat.GetCategoryOrDefault())
                .OrderBy(group => group.Key)
                .Select(group => new CategoryGroup(group.Key, group.OrderBy(cheat => cheat.GetLabel()).ToList()))
                .ToList();

            float viewHeight = CalculateViewHeight(groups);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            DrawVisibleRows(viewRect, outRect.height, groups);
            Widgets.EndScrollView();
        }

        private void DrawVisibleRows(Rect viewRect, float visibleHeight, List<CategoryGroup> groups)
        {
            float y = 0f;
            float visibleTop = scrollPosition.y;
            float visibleBottom = scrollPosition.y + visibleHeight;

            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                CategoryGroup group = groups[groupIndex];

                Rect headerRect = new Rect(viewRect.x, y, viewRect.width, CategoryHeaderHeight);
                if (RowIsVisible(y, CategoryHeaderHeight, visibleTop, visibleBottom))
                {
                    DrawCategoryHeader(headerRect, group.CategoryLabel, group.Cheats.Count);
                }

                y += CategoryHeaderHeight + RowSpacing;

                for (int cheatIndex = 0; cheatIndex < group.Cheats.Count; cheatIndex++)
                {
                    CheatDefinition cheat = group.Cheats[cheatIndex];
                    Rect cardRect = new Rect(viewRect.x, y, viewRect.width, CheatCardHeight);
                    if (RowIsVisible(y, CheatCardHeight, visibleTop, visibleBottom))
                    {
                        DrawCheatCard(cardRect, cheat, cheatIndex % 2 == 0);
                    }

                    y += CheatCardHeight + RowSpacing;
                }
            }
        }

        private void DrawCategoryHeader(Rect rect, string categoryLabel, int count)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1f, rect.width);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "CheatMenu.Window.CategoryWithCount".Translate(categoryLabel, count));
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawCheatCard(Rect rect, CheatDefinition cheat, bool drawAltBackground)
        {
            if (drawAltBackground)
            {
                Widgets.DrawAltRect(rect);
            }

            Widgets.DrawHighlightIfMouseover(rect);

            Rect contentRect = new Rect(
                rect.x + SidePadding,
                rect.y + SidePadding,
                rect.width - (SidePadding * 2f),
                rect.height - (SidePadding * 2f));

            Rect buttonRect = new Rect(
                contentRect.xMax - ExecuteButtonWidth,
                contentRect.y + ((contentRect.height - ExecuteButtonHeight) * 0.5f),
                ExecuteButtonWidth,
                ExecuteButtonHeight);

            Rect infoRect = new Rect(
                contentRect.x,
                contentRect.y,
                contentRect.width - ExecuteButtonWidth - 12f,
                contentRect.height);

            DrawCheatInfo(infoRect, cheat);
            DrawExecuteButton(buttonRect, cheat);

            TooltipHandler.TipRegion(rect, cheat.GetDescription());
        }

        private void DrawCheatInfo(Rect rect, CheatDefinition cheat)
        {
            Rect labelRect = new Rect(rect.x, rect.y, rect.width, 24f);
            Text.Font = GameFont.Small;
            Widgets.Label(labelRect, cheat.GetLabel());

            Rect descRect = new Rect(rect.x, labelRect.yMax, rect.width, 32f);
            Text.Font = GameFont.Tiny;
            Widgets.Label(descRect, cheat.GetDescription());

            Rect metaRect = new Rect(rect.x, rect.yMax - 20f, rect.width, 20f);
            Text.Font = GameFont.Tiny;
            DrawTypeIndicators(metaRect, cheat);
            Text.Font = GameFont.Small;
        }

        private void DrawTypeIndicators(Rect rect, CheatDefinition cheat)
        {
            List<string> typeLabels = new List<string>();
            if (cheat.Kinds.HasFlag(CheatKinds.Action))
            {
                typeLabels.Add("CheatMenu.Type.Action".Translate());
            }

            if (cheat.Kinds.HasFlag(CheatKinds.Tool))
            {
                typeLabels.Add("CheatMenu.Type.Tool".Translate());
            }

            if (cheat.Kinds.HasFlag(CheatKinds.Window))
            {
                typeLabels.Add("CheatMenu.Type.Window".Translate());
            }

            if (cheat.IsComposite)
            {
                typeLabels.Add("CheatMenu.Type.Composite".Translate());
            }

            string leftText = string.Join("  ", typeLabels);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width * 0.55f, rect.height), leftText);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, rect.height), GetExecutionHint(cheat));
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawExecuteButton(Rect rect, CheatDefinition cheat)
        {
            bool needsMap = cheat.RequiresMap && Find.CurrentMap == null;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = !needsMap;

            if (Widgets.ButtonText(rect, GetExecuteButtonLabel(cheat)))
            {
                CheatFlowRunner.Execute(cheat);
            }

            GUI.enabled = previousEnabled;

            if (needsMap)
            {
                TooltipHandler.TipRegion(rect, "CheatMenu.Window.RequiresMapTooltip".Translate());
            }
        }

        private string GetExecuteButtonLabel(CheatDefinition cheat)
        {
            if (cheat.IsComposite)
            {
                return "CheatMenu.Button.StartFlow".Translate();
            }

            if (cheat.Kinds == CheatKinds.Action)
            {
                return "CheatMenu.Button.ExecuteNow".Translate();
            }

            if (cheat.Kinds == CheatKinds.Tool)
            {
                return "CheatMenu.Button.StartTargeting".Translate();
            }

            if (cheat.Kinds == CheatKinds.Window)
            {
                return "CheatMenu.Button.OpenWindow".Translate();
            }

            return "CheatMenu.Button.Run".Translate();
        }

        private string GetExecutionHint(CheatDefinition cheat)
        {
            if (cheat.IsComposite)
            {
                return "CheatMenu.Hint.Composite".Translate();
            }

            if (cheat.Kinds == CheatKinds.Action)
            {
                return "CheatMenu.Hint.Instant".Translate();
            }

            if (cheat.Kinds == CheatKinds.Tool)
            {
                return "CheatMenu.Hint.Targeting".Translate();
            }

            if (cheat.Kinds == CheatKinds.Window)
            {
                return "CheatMenu.Hint.OpensWindow".Translate();
            }

            return string.Empty;
        }

        private bool MatchesSearch(CheatDefinition cheat)
        {
            if (searchText.NullOrEmpty())
            {
                return true;
            }

            string needle = searchText.Trim().ToLowerInvariant();
            if (needle.Length == 0)
            {
                return true;
            }

            return cheat.GetLabel().ToLowerInvariant().Contains(needle)
                || cheat.GetDescription().ToLowerInvariant().Contains(needle)
                || cheat.GetCategoryOrDefault().ToLowerInvariant().Contains(needle);
        }

        private static bool RowIsVisible(float y, float height, float visibleTop, float visibleBottom)
        {
            return y + height >= visibleTop && y <= visibleBottom;
        }

        private static float CalculateViewHeight(List<CategoryGroup> groups)
        {
            float height = 0f;
            for (int i = 0; i < groups.Count; i++)
            {
                height += CategoryHeaderHeight + RowSpacing;
                height += groups[i].Cheats.Count * (CheatCardHeight + RowSpacing);
            }

            return height + 8f;
        }

        private sealed class CategoryGroup
        {
            public CategoryGroup(string categoryLabel, List<CheatDefinition> cheats)
            {
                CategoryLabel = categoryLabel;
                Cheats = cheats;
            }

            public string CategoryLabel { get; }

            public List<CheatDefinition> Cheats { get; }
        }
    }
}
