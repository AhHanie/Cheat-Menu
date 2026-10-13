using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class MapTerrainSelectionWindow : SearchableSelectionWindow<TerrainDef>
    {
        private const string SearchControlNameConst = "CheatMenu.MapSetTerrainRect.SearchField";

        private readonly Action<TerrainDef> onTerrainSelected;
        private readonly List<TerrainDef> allTerrains;

        public MapTerrainSelectionWindow(Action<TerrainDef> onTerrainSelected)
            : base(new Vector2(920f, 700f))
        {
            this.onTerrainSelected = onTerrainSelected;
            allTerrains = BuildTerrainList();
        }

        protected override string TitleKey => "CheatMenu.MapSetTerrainRect.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.MapSetTerrainRect.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.MapSetTerrainRect.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.MapSetTerrainRect.Window.SelectButton";

        protected override IReadOnlyList<TerrainDef> Options => allTerrains;

        protected override void DrawItemInfo(Rect rect, TerrainDef terrainDef)
        {
            if (terrainDef == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), terrainDef.defName);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.MapSetTerrainRect.Window.InfoLine".Translate(terrainDef.label ?? terrainDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(TerrainDef terrainDef, string needle)
        {
            if (terrainDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string defName = (terrainDef.defName ?? string.Empty).ToLowerInvariant();
            string label = (terrainDef.label ?? string.Empty).ToLowerInvariant();

            return defName.Contains(needle) || label.Contains(needle);
        }

        protected override void OnItemSelected(TerrainDef terrainDef)
        {
            Close();
            onTerrainSelected?.Invoke(terrainDef);
        }

        private static List<TerrainDef> BuildTerrainList()
        {
            return DefDatabase<TerrainDef>.AllDefsListForReading
                .Where(terrainDef => terrainDef != null && !terrainDef.temporary)
                .OrderBy(terrainDef => terrainDef.defName)
                .ToList();
        }
    }
}
