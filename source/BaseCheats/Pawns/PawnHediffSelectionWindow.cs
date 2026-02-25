using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnHediffSelectionWindow : SearchableSelectionWindow<HediffDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnAddHediff.SearchField";

        private readonly Action<HediffDef> onHediffSelected;
        private readonly List<HediffDef> allOptions;

        public PawnHediffSelectionWindow(Action<HediffDef> onHediffSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onHediffSelected = onHediffSelected;
            allOptions = BuildHediffList();
        }

        protected override string TitleKey => "CheatMenu.PawnAddHediff.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnAddHediff.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnAddHediff.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnAddHediff.Window.SelectButton";

        protected override IReadOnlyList<HediffDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, HediffDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), BuildHediffDefLabel(option));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnAddHediff.Window.InfoLine".Translate(option.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(HediffDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string hediffLabel = BuildHediffDefLabel(option).ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();

            return hediffLabel.Contains(needle)
                || defName.Contains(needle);
        }

        protected override void OnItemSelected(HediffDef option)
        {
            Close();
            onHediffSelected?.Invoke(option);
        }

        private static List<HediffDef> BuildHediffList()
        {
            List<HediffDef> result = new List<HediffDef>();
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                result.Add(hediffDef);
            }

            return result
                .OrderBy(option => option.defName)
                .ToList();
        }

        private string BuildHediffDefLabel(HediffDef hediffDef)
        {
            if (!hediffDef.debugLabelExtra.NullOrEmpty())
            {
                return hediffDef.LabelCap + " (" + hediffDef.debugLabelExtra + ")";
            }

            return hediffDef.LabelCap;
        }
    }
}
