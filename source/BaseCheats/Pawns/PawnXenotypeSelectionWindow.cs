using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnXenotypeSelectionWindow : SearchableSelectionWindow<XenotypeDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetXenotype.SearchField";

        private readonly Action<XenotypeDef> onXenotypeSelected;
        private readonly List<XenotypeDef> allOptions;

        public PawnXenotypeSelectionWindow(Action<XenotypeDef> onXenotypeSelected)
            : base(new Vector2(920f, 700f))
        {
            this.onXenotypeSelected = onXenotypeSelected;
            allOptions = BuildXenotypeList();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override string TitleKey => "CheatMenu.PawnSetXenotype.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetXenotype.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetXenotype.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetXenotype.Window.SelectButton";

        protected override IReadOnlyList<XenotypeDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, XenotypeDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetXenotype.Window.InfoLine".Translate(option.defName));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, XenotypeDef option)
        {
            Texture2D icon = option.Icon;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }

        protected override bool MatchesSearch(XenotypeDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string xenotypeLabel = option.label.ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();

            return xenotypeLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(XenotypeDef option)
        {
            Close();
            onXenotypeSelected?.Invoke(option);
        }

        private static List<XenotypeDef> BuildXenotypeList()
        {
            return DefDatabase<XenotypeDef>.AllDefsListForReading
                .OrderBy(option => option.defName)
                .ToList();
        }
    }
}
