using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class MapGameConditionSelectionWindow : SearchableSelectionWindow<GameConditionDef>
    {
        private const string SearchControlNameConst = "CheatMenu.MapGameCondition.SearchField";

        private readonly Action<GameConditionDef> onConditionSelected;
        private readonly bool onlyActiveConditions;
        private readonly List<GameConditionDef> conditions;

        public MapGameConditionSelectionWindow(Action<GameConditionDef> onConditionSelected, bool onlyActiveConditions)
            : base(new Vector2(920f, 700f))
        {
            this.onConditionSelected = onConditionSelected;
            this.onlyActiveConditions = onlyActiveConditions;
            conditions = BuildConditionList(onlyActiveConditions);
        }

        protected override string TitleKey => onlyActiveConditions
            ? "CheatMenu.MapRemoveGameCondition.Window.Title"
            : "CheatMenu.MapAddGameCondition.Window.Title";

        protected override string SearchTooltipKey => onlyActiveConditions
            ? "CheatMenu.MapRemoveGameCondition.Window.SearchTooltip"
            : "CheatMenu.MapAddGameCondition.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => onlyActiveConditions
            ? "CheatMenu.MapRemoveGameCondition.Window.NoMatches"
            : "CheatMenu.MapAddGameCondition.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.MapGameCondition.Window.SelectButton";

        protected override IReadOnlyList<GameConditionDef> Options => conditions;

        protected override bool MatchesSearch(GameConditionDef conditionDef, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = conditionDef.label.ToLowerInvariant();
            string defName = conditionDef.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, GameConditionDef conditionDef)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), conditionDef.LabelCap);

            string infoLine;
            if (onlyActiveConditions && TryGetActiveDurationLabel(conditionDef, out string activeDurationLabel))
            {
                infoLine = "CheatMenu.MapGameCondition.Window.InfoLineActive".Translate(conditionDef.defName, activeDurationLabel);
            }
            else
            {
                infoLine = "CheatMenu.MapGameCondition.Window.InfoLineDefName".Translate(conditionDef.defName);
            }

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(GameConditionDef conditionDef)
        {
            Close();
            onConditionSelected?.Invoke(conditionDef);
        }

        private static List<GameConditionDef> BuildConditionList(bool activeOnly)
        {
            if (!activeOnly)
            {
                return DefDatabase<GameConditionDef>.AllDefsListForReading
                    .OrderBy(conditionDef => conditionDef.label)
                    .ThenBy(conditionDef => conditionDef.defName)
                    .ToList();
            }

            Map map = Find.CurrentMap;
            return map.gameConditionManager.ActiveConditions
                .Select(condition => condition.def)
                .Distinct()
                .OrderBy(conditionDef => conditionDef.label)
                .ThenBy(conditionDef => conditionDef.defName)
                .ToList();
        }

        private static bool TryGetActiveDurationLabel(GameConditionDef conditionDef, out string durationLabel)
        {
            Map map = Find.CurrentMap;
            GameCondition activeCondition = map.gameConditionManager.GetActiveCondition(conditionDef);
            if (activeCondition == null)
            {
                durationLabel = string.Empty;
                return false;
            }

            durationLabel = activeCondition.Permanent
                ? "CheatMenu.MapAddGameCondition.DurationWindow.PermanentOption".Translate().ToString()
                : (activeCondition.Duration.ToStringTicksToPeriod() ?? activeCondition.Duration.ToString());
            return true;
        }
    }
}
