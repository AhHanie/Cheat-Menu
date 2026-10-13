using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class MapNaturalRockSelectionWindow : SearchableSelectionWindow<ThingDef>
    {
        private const string SearchControlNameConst = "CheatMenu.MapExtendMountain.RockWindow.SearchField";

        private readonly Action<ThingDef> onRockSelected;
        private readonly List<ThingDef> allRocks;

        public MapNaturalRockSelectionWindow(Action<ThingDef> onRockSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onRockSelected = onRockSelected;
            allRocks = BuildRockList();
        }

        protected override string TitleKey => "CheatMenu.MapExtendMountain.RockWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.MapExtendMountain.RockWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.MapExtendMountain.RockWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.MapExtendMountain.RockWindow.SelectButton";

        protected override IReadOnlyList<ThingDef> Options => allRocks;

        protected override void DrawItemInfo(Rect rect, ThingDef rockDef)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), rockDef.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.MapExtendMountain.RockWindow.InfoLine".Translate(rockDef.defName, rockDef.building.naturalTerrain.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(ThingDef rockDef, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string defName = (rockDef.defName ?? string.Empty).ToLowerInvariant();
            string label = (rockDef.label ?? string.Empty).ToLowerInvariant();

            return defName.Contains(needle) || label.Contains(needle);
        }

        protected override void OnItemSelected(ThingDef rockDef)
        {
            Close();
            onRockSelected?.Invoke(rockDef);
        }

        private static List<ThingDef> BuildRockList()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(def => def.IsNonResourceNaturalRock && def.mineable && def.building.naturalTerrain != null)
                .OrderBy(def => def.label ?? def.defName)
                .ThenBy(def => def.defName)
                .ToList();
        }
    }
}
