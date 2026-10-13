using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnCurrentHediffSelectionWindowKeys
    {
        public string TitleKey;
        public string SearchTooltipKey;
        public string SearchControlName;
        public string NoMatchesKey;
        public string SelectButtonKey;
        public string InfoLineKey;
        public string BodyPartSuffixKey;

        public static PawnCurrentHediffSelectionWindowKeys Default => new PawnCurrentHediffSelectionWindowKeys
        {
            TitleKey = "CheatMenu.PawnSetHediffStage.HediffWindow.Title",
            SearchTooltipKey = "CheatMenu.PawnSetHediffStage.HediffWindow.SearchTooltip",
            SearchControlName = "CheatMenu.PawnSetHediffStage.HediffWindow.SearchField",
            NoMatchesKey = "CheatMenu.PawnSetHediffStage.HediffWindow.NoMatches",
            SelectButtonKey = "CheatMenu.PawnSetHediffStage.HediffWindow.SelectButton",
            InfoLineKey = "CheatMenu.PawnSetHediffStage.HediffWindow.InfoLine",
            BodyPartSuffixKey = "CheatMenu.PawnSetHediffStage.HediffWindow.BodyPartSuffix",
        };
    }

    public sealed class PawnCurrentHediffSelectionOption
    {
        public PawnCurrentHediffSelectionOption(Hediff hediff)
        {
            Hediff = hediff;
        }

        public Hediff Hediff { get; }
    }

    public class PawnCurrentHediffSelectionWindow : SearchableSelectionWindow<PawnCurrentHediffSelectionOption>
    {
        private readonly PawnCurrentHediffSelectionWindowKeys keys;
        private readonly Action<Hediff> onHediffSelected;
        private readonly string pawnLabel;
        private readonly List<PawnCurrentHediffSelectionOption> allOptions;

        public PawnCurrentHediffSelectionWindow(
            Pawn sourcePawn,
            IEnumerable<Hediff> hediffs,
            Action<Hediff> onHediffSelected,
            PawnCurrentHediffSelectionWindowKeys keys = null)
            : base(new Vector2(860f, 700f))
        {
            this.keys = keys ?? PawnCurrentHediffSelectionWindowKeys.Default;
            this.onHediffSelected = onHediffSelected;
            pawnLabel = sourcePawn.LabelShortCap;
            allOptions = BuildOptions(hediffs);
        }

        protected override string TitleKey => keys.TitleKey;

        protected override string SearchTooltipKey => keys.SearchTooltipKey;

        protected override string SearchControlName => keys.SearchControlName;

        protected override string NoMatchesKey => keys.NoMatchesKey;

        protected override string SelectButtonKey => keys.SelectButtonKey;

        protected override IReadOnlyList<PawnCurrentHediffSelectionOption> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(pawnLabel);
        }

        protected override void DrawItemInfo(Rect rect, PawnCurrentHediffSelectionOption option)
        {
            Hediff hediff = option.Hediff;
            string mainLabel = GetHediffDisplayLabel(hediff);
            if (hediff.Part != null)
            {
                mainLabel += keys.BodyPartSuffixKey.Translate(hediff.Part.LabelCap);
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), mainLabel);

            string infoLine = keys.InfoLineKey.Translate(
                hediff.def.defName,
                hediff.CurStageIndex,
                hediff.Severity.ToString("0.###"));

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnCurrentHediffSelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            Hediff hediff = option.Hediff;
            string label = GetHediffDisplayLabel(hediff).ToLowerInvariant();
            string defName = hediff.def.defName.ToLowerInvariant();
            string partLabel = hediff.Part != null ? hediff.Part.Label.ToLowerInvariant() : string.Empty;
            string stageLabel = GetStageLabel(hediff.CurStage, hediff.CurStageIndex).ToLowerInvariant();
            string severity = hediff.Severity.ToString("0.###");

            return label.Contains(needle)
                || defName.Contains(needle)
                || (!partLabel.NullOrEmpty() && partLabel.Contains(needle))
                || stageLabel.Contains(needle)
                || severity.Contains(needle);
        }

        protected override void OnItemSelected(PawnCurrentHediffSelectionOption option)
        {
            Close();
            onHediffSelected?.Invoke(option.Hediff);
        }

        private static List<PawnCurrentHediffSelectionOption> BuildOptions(IEnumerable<Hediff> hediffs)
        {
            return hediffs
                .OrderBy(h => GetHediffDisplayLabel(h))
                .ThenBy(h => h.Part != null ? h.Part.Label : string.Empty)
                .ThenBy(h => h.def.defName)
                .ThenBy(h => h.CurStageIndex)
                .Select(h => new PawnCurrentHediffSelectionOption(h))
                .ToList();
        }

        internal static string GetHediffDisplayLabel(Hediff hediff)
        {
            string label = hediff.LabelCap;
            if (label.NullOrEmpty())
            {
                label = hediff.def.LabelCap;
            }
            if (label.NullOrEmpty())
            {
                label = hediff.def.defName;
            }
            if (!hediff.def.debugLabelExtra.NullOrEmpty())
            {
                label = label + " (" + hediff.def.debugLabelExtra + ")";
            }
            return label;
        }

        internal static string GetStageLabel(HediffStage stage, int index)
        {
            if (stage == null)
            {
                return "CheatMenu.PawnSetHediffStage.StageWindow.UnlabeledStage".Translate() + " " + index;
            }

            string label = stage.label;
            if (label.NullOrEmpty())
            {
                label = stage.overrideLabel;
            }
            if (label.NullOrEmpty())
            {
                label = "CheatMenu.PawnSetHediffStage.StageWindow.UnlabeledStage".Translate();
            }
            return label;
        }
    }
}
