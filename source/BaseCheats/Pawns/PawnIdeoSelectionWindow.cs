using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnIdeoSelectionWindow : SearchableSelectionWindow<Ideo>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetIdeo.SearchField";

        private readonly Action<Ideo> onIdeoSelected;
        private readonly List<Ideo> allOptions;

        public PawnIdeoSelectionWindow(Action<Ideo> onIdeoSelected)
            : base(new Vector2(900f, 700f))
        {
            this.onIdeoSelected = onIdeoSelected;
            allOptions = BuildIdeoList();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override string TitleKey => "CheatMenu.PawnSetIdeo.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetIdeo.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetIdeo.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetIdeo.Window.SelectButton";

        protected override IReadOnlyList<Ideo> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, Ideo option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.name);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetIdeo.Window.InfoLine".Translate(GetTypeLabel(option)));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, Ideo option)
        {
            Texture2D icon = option.Icon;
            Color previousColor = GUI.color;
            GUI.color = option.Color;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        protected override bool MatchesSearch(Ideo option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            return option.name.ToLowerInvariant().Contains(needle);
        }

        protected override void OnItemSelected(Ideo option)
        {
            Close();
            onIdeoSelected?.Invoke(option);
        }

        private static List<Ideo> BuildIdeoList()
        {
            return Find.IdeoManager.IdeosListForReading
                .OrderBy(option => option.name)
                .ToList();
        }

        private static string GetTypeLabel(Ideo ideo)
        {
            return ideo.classicMode
                ? "CheatMenu.PawnSetIdeo.Window.Type.Classic".Translate()
                : "CheatMenu.PawnSetIdeo.Window.Type.Fluid".Translate();
        }
    }
}
