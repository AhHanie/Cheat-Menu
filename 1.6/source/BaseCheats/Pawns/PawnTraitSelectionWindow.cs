using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class TraitSelection
    {
        public TraitSelection(TraitDef traitDef, int degree, string label)
        {
            TraitDef = traitDef;
            Degree = degree;
            Label = label;
        }

        public TraitDef TraitDef { get; }

        public int Degree { get; }

        public string Label { get; }
    }

    public class PawnTraitSelectionWindow : SearchableSelectionWindow<TraitSelection>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnGiveTrait.SearchField";

        private readonly Action<TraitSelection> onTraitSelected;
        private readonly List<TraitSelection> allTraits;

        public PawnTraitSelectionWindow(Action<TraitSelection> onTraitSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onTraitSelected = onTraitSelected;
            allTraits = BuildTraitList();
        }

        protected override string TitleKey => "CheatMenu.PawnGiveTrait.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnGiveTrait.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnGiveTrait.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnGiveTrait.Window.SelectButton";

        protected override IReadOnlyList<TraitSelection> Options => allTraits;

        protected override void DrawItemInfo(Rect rect, TraitSelection selection)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), selection.Label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnGiveTrait.Window.InfoLine".Translate(selection.TraitDef.defName, selection.Degree));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(TraitSelection selection, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string traitLabel = selection.Label;
            string defName = selection.TraitDef.defName.ToLowerInvariant();

            return traitLabel.Contains(needle)
                || defName.Contains(needle);
        }

        protected override void OnItemSelected(TraitSelection selection)
        {
            Close();
            onTraitSelected?.Invoke(selection);
        }

        private static List<TraitSelection> BuildTraitList()
        {
            List<TraitSelection> result = new List<TraitSelection>();
            foreach (TraitDef traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                for (int j = 0; j < traitDef.degreeDatas.Count; j++)
                {
                    TraitDegreeData degreeData = traitDef.degreeDatas[j];
                    string degreeLabel = degreeData.label;
                    string displayLabel = degreeLabel + " (" + degreeData.degree + ")";
                    result.Add(new TraitSelection(traitDef, degreeData.degree, displayLabel));
                }
            }

            return result
                .OrderBy(option => option.Label)
                .ThenBy(option => option.TraitDef.defName)
                .ToList();
        }
    }
}
