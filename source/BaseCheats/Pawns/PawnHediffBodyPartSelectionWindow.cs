using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnHediffBodyPartSelectionOption
    {
        public PawnHediffBodyPartSelectionOption(BodyPartRecord bodyPart, string displayLabel, bool isWholeBody)
        {
            BodyPart = bodyPart;
            DisplayLabel = displayLabel;
            IsWholeBody = isWholeBody;
        }

        public BodyPartRecord BodyPart { get; }

        public string DisplayLabel { get; }

        public bool IsWholeBody { get; }
    }

    public class PawnHediffBodyPartSelectionWindow : SearchableSelectionWindow<PawnHediffBodyPartSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnAddHediff.BodyPart.SearchField";

        private readonly Action<PawnHediffBodyPartSelectionOption> onPartSelected;
        private readonly string pawnLabel;
        private readonly string hediffLabel;
        private readonly List<PawnHediffBodyPartSelectionOption> allOptions;

        public PawnHediffBodyPartSelectionWindow(
            Pawn pawn,
            HediffDef hediffDef,
            Action<PawnHediffBodyPartSelectionOption> onPartSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onPartSelected = onPartSelected;
            pawnLabel = pawn.LabelShortCap;
            hediffLabel = hediffDef.LabelCap;
            allOptions = BuildBodyPartOptions(pawn);
        }

        protected override string TitleKey => "CheatMenu.PawnAddHediff.BodyPartWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnAddHediff.BodyPartWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnAddHediff.BodyPartWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnAddHediff.BodyPartWindow.SelectButton";

        protected override IReadOnlyList<PawnHediffBodyPartSelectionOption> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(pawnLabel, hediffLabel);
        }

        protected override void DrawItemInfo(Rect rect, PawnHediffBodyPartSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            string infoText = option.IsWholeBody
                ? "CheatMenu.PawnAddHediff.BodyPartWindow.WholeBodyInfo".Translate().ToString()
                : "CheatMenu.PawnAddHediff.BodyPartWindow.InfoLine".Translate(option.BodyPart.def.defName).ToString();

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoText);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnHediffBodyPartSelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            if (option.IsWholeBody)
            {
                return option.DisplayLabel.ToLowerInvariant().Contains(needle);
            }

            string displayLabel = option.DisplayLabel.ToLowerInvariant();
            string partLabel = option.BodyPart.def.label.ToLowerInvariant();
            string defName = option.BodyPart.def.defName.ToLowerInvariant();

            return displayLabel.Contains(needle)
                || partLabel.Contains(needle)
                || defName.Contains(needle);
        }

        protected override void OnItemSelected(PawnHediffBodyPartSelectionOption option)
        {
            Close();
            onPartSelected?.Invoke(option);
        }

        private static List<PawnHediffBodyPartSelectionOption> BuildBodyPartOptions(Pawn pawn)
        {
            List<PawnHediffBodyPartSelectionOption> result = new List<PawnHediffBodyPartSelectionOption>
            {
                new PawnHediffBodyPartSelectionOption(
                    bodyPart: null,
                    displayLabel: "CheatMenu.PawnAddHediff.Part.WholeBody".Translate(),
                    isWholeBody: true)
            };

            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                if (pawn.health.hediffSet.PartIsMissing(part))
                {
                    continue;
                }

                string displayLabel = part.LabelCap;
                if (part.parent != null)
                {
                    displayLabel = displayLabel + " (" + part.parent.LabelCap + ")";
                }

                result.Add(new PawnHediffBodyPartSelectionOption(part, displayLabel, false));
            }

            return result
                .OrderBy(option => option.IsWholeBody ? 0 : 1)
                .ThenBy(option => option.DisplayLabel)
                .ToList();
        }
    }
}
