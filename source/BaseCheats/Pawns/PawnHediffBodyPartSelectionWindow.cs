using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnHediffBodyPartSelectionOption
    {
        public PawnHediffBodyPartSelectionOption(BodyPartRecord bodyPart, string displayLabel, bool isWholeBody)
        {
            BodyPart = bodyPart;
            DisplayLabel = displayLabel ?? string.Empty;
            IsWholeBody = isWholeBody;
        }

        public BodyPartRecord BodyPart { get; }

        public string DisplayLabel { get; }

        public bool IsWholeBody { get; }
    }

    public sealed class PawnHediffBodyPartSelectionWindow : SearchableSelectionWindow<PawnHediffBodyPartSelectionOption>
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
            pawnLabel = pawn?.LabelShortCap ?? string.Empty;
            hediffLabel = GetHediffDisplayLabel(hediffDef);
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
            if (option == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            string infoText = option.IsWholeBody
                ? "CheatMenu.PawnAddHediff.BodyPartWindow.WholeBodyInfo".Translate().ToString()
                : "CheatMenu.PawnAddHediff.BodyPartWindow.InfoLine".Translate(option.BodyPart?.def?.defName ?? string.Empty).ToString();

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoText);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnHediffBodyPartSelectionOption option, string needle)
        {
            if (option == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string partLabel = (option.BodyPart?.def?.label ?? string.Empty).ToLowerInvariant();
            string defName = (option.BodyPart?.def?.defName ?? string.Empty).ToLowerInvariant();
            string wholeBodyLabel = "CheatMenu.PawnAddHediff.Part.WholeBody".Translate().ToString().ToLowerInvariant();

            return displayLabel.Contains(needle)
                || partLabel.Contains(needle)
                || defName.Contains(needle)
                || (option.IsWholeBody && wholeBodyLabel.Contains(needle));
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

            if (pawn?.health?.hediffSet == null || pawn.RaceProps?.body?.AllParts == null)
            {
                return result;
            }

            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                if (part == null || pawn.health.hediffSet.PartIsMissing(part))
                {
                    continue;
                }

                string displayLabel = part.LabelCap;
                if (displayLabel.NullOrEmpty())
                {
                    displayLabel = part.def?.label ?? part.def?.defName ?? string.Empty;
                }

                if (part.parent != null)
                {
                    string parentLabel = part.parent.LabelCap;
                    if (!parentLabel.NullOrEmpty() && !string.Equals(parentLabel, displayLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        displayLabel = displayLabel + " (" + parentLabel + ")";
                    }
                }

                result.Add(new PawnHediffBodyPartSelectionOption(part, displayLabel, false));
            }

            return result
                .OrderBy(option => option.IsWholeBody ? 0 : 1)
                .ThenBy(option => option.DisplayLabel)
                .ToList();
        }

        private static string GetHediffDisplayLabel(HediffDef hediffDef)
        {
            if (hediffDef == null)
            {
                return string.Empty;
            }

            string label = hediffDef.LabelCap;
            if (label.NullOrEmpty())
            {
                label = hediffDef.defName ?? hediffDef.hediffClass?.ToStringSafe() ?? string.Empty;
            }

            if (!hediffDef.debugLabelExtra.NullOrEmpty())
            {
                label = label + " (" + hediffDef.debugLabelExtra + ")";
            }

            return label;
        }
    }
}
