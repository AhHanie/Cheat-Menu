using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class XenotypeSelectionOption
    {
        public XenotypeSelectionOption(XenotypeDef xenotypeDef, string displayLabel)
        {
            XenotypeDef = xenotypeDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public XenotypeDef XenotypeDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnXenotypeSelectionWindow : SearchableSelectionWindow<XenotypeSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetXenotype.SearchField";

        private readonly Action<XenotypeSelectionOption> onXenotypeSelected;
        private readonly List<XenotypeSelectionOption> allOptions;

        public PawnXenotypeSelectionWindow(Action<XenotypeSelectionOption> onXenotypeSelected)
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

        protected override IReadOnlyList<XenotypeSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, XenotypeSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnSetXenotype.Window.InfoLine".Translate(option.XenotypeDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, XenotypeSelectionOption option)
        {
            Texture2D icon = option.XenotypeDef?.Icon ?? BaseContent.BadTex;
            if (icon == null)
            {
                icon = BaseContent.BadTex;
            }

            //Color previousColor = GUI.color;
            //GUI.color = option?.XenotypeDef?.IconColor ?? Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            //GUI.color = previousColor;
        }

        protected override bool MatchesSearch(XenotypeSelectionOption option, string needle)
        {
            XenotypeDef xenotypeDef = option.XenotypeDef;
            if (xenotypeDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string xenotypeLabel = (xenotypeDef.label ?? string.Empty).ToLowerInvariant();
            string defName = xenotypeDef.defName.ToLowerInvariant();

            return displayLabel.Contains(needle) || xenotypeLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(XenotypeSelectionOption option)
        {
            Close();
            onXenotypeSelected?.Invoke(option);
        }

        private static List<XenotypeSelectionOption> BuildXenotypeList()
        {
            return DefDatabase<XenotypeDef>.AllDefsListForReading
                .Where(xenotypeDef => xenotypeDef != null)
                .Select(xenotypeDef => new XenotypeSelectionOption(
                    xenotypeDef,
                    xenotypeDef.defName))
                .OrderBy(option => option.XenotypeDef.defName)
                .ToList();
        }
    }
}
