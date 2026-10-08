using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public enum MapNaturalRockRoofMode
    {
        OverheadMountain,
        ThinRockRoof,
        NoChange
    }

    public class MapNaturalRockRoofSelectionWindow : SearchableSelectionWindow<MapNaturalRockRoofMode>
    {
        private static readonly MapNaturalRockRoofMode[] AllModes =
        {
            MapNaturalRockRoofMode.OverheadMountain,
            MapNaturalRockRoofMode.ThinRockRoof,
            MapNaturalRockRoofMode.NoChange
        };

        private readonly Action<MapNaturalRockRoofMode> onModeSelected;

        public MapNaturalRockRoofSelectionWindow(Action<MapNaturalRockRoofMode> onModeSelected)
            : base(new Vector2(860f, 440f), rowHeight: 84f)
        {
            this.onModeSelected = onModeSelected;
        }

        public static string GetModeKey(MapNaturalRockRoofMode mode)
        {
            return "CheatMenu.MapExtendMountain.RoofWindow." + mode;
        }

        protected override bool ShowSearchRow => false;

        protected override string TitleKey => "CheatMenu.MapExtendMountain.RoofWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.MapExtendMountain.RoofWindow.Title";

        protected override string SearchControlName => "CheatMenu.MapExtendMountain.RoofWindow";

        protected override string NoMatchesKey => "CheatMenu.MapExtendMountain.RoofWindow.NoOptions";

        protected override string SelectButtonKey => "CheatMenu.MapExtendMountain.RoofWindow.SelectButton";

        protected override IReadOnlyList<MapNaturalRockRoofMode> Options => AllModes;

        protected override bool MatchesSearch(MapNaturalRockRoofMode mode, string needle)
        {
            return true;
        }

        protected override void DrawItemInfo(Rect rect, MapNaturalRockRoofMode mode)
        {
            string key = GetModeKey(mode);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), key.Translate());

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.y + 26f, rect.width, rect.height - 26f),
                (key + ".Info").Translate());
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(MapNaturalRockRoofMode mode)
        {
            Close();
            onModeSelected?.Invoke(mode);
        }
    }
}
