using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnPassionSelectionOption
    {
        public PawnPassionSelectionOption(Passion passion, string displayLabel)
        {
            Passion = passion;
            DisplayLabel = displayLabel;
        }

        public Passion Passion { get; }

        public string DisplayLabel { get; }
    }

    public class PawnPassionSelectionWindow : SearchableSelectionWindow<PawnPassionSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetPassion.SearchField";
        private const float RowHeight = 40f;

        private readonly Action<PawnPassionSelectionOption> onPassionSelected;
        private readonly List<PawnPassionSelectionOption> allOptions;

        public PawnPassionSelectionWindow(Action<PawnPassionSelectionOption> onPassionSelected)
            : base(new Vector2(420f, 300f), rowHeight: RowHeight)
        {
            this.onPassionSelected = onPassionSelected;
            allOptions = BuildPassions();
        }

        protected override bool ShowSearchRow => false;

        protected override float SelectButtonWidth => 96f;

        protected override string TitleKey => "CheatMenu.PawnSetPassion.PassionWindow.Title";

        protected override string SearchTooltipKey => string.Empty;

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetPassion.PassionWindow.NoOptions";

        protected override string SelectButtonKey => "CheatMenu.PawnSetPassion.PassionWindow.SelectButton";

        protected override IReadOnlyList<PawnPassionSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnPassionSelectionOption option)
        {
            Widgets.Label(rect, option.DisplayLabel);
        }

        protected override bool MatchesSearch(PawnPassionSelectionOption option, string needle)
        {
            return true;
        }

        protected override void OnItemSelected(PawnPassionSelectionOption option)
        {
            Close();
            onPassionSelected?.Invoke(option);
        }

        private static List<PawnPassionSelectionOption> BuildPassions()
        {
            return Enum.GetValues(typeof(Passion))
                .Cast<Passion>()
                .OrderBy(passion => (int)passion)
                .Select(passion => new PawnPassionSelectionOption(passion, GetPassionLabel(passion)))
                .ToList();
        }

        private static string GetPassionLabel(Passion passion)
        {
            switch (passion)
            {
                case Passion.None:
                    return "CheatMenu.PawnSetPassion.Passion.None".Translate();
                case Passion.Minor:
                    return "CheatMenu.PawnSetPassion.Passion.Minor".Translate();
                case Passion.Major:
                    return "CheatMenu.PawnSetPassion.Passion.Major".Translate();
                default:
                    return passion.ToString();
            }
        }
    }
}
