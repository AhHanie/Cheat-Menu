using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnHediffStageSelectionOption
    {
        public PawnHediffStageSelectionOption(int index, HediffStage stage)
        {
            Index = index;
            Stage = stage;
        }

        public int Index { get; }
        public HediffStage Stage { get; }
        public float MinSeverity => Stage.minSeverity;
    }

    public class PawnHediffStageSelectionWindow : SearchableSelectionWindow<PawnHediffStageSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnSetHediffStage.StageWindow.SearchField";

        private readonly Action<PawnHediffStageSelectionOption> onStageSelected;
        private readonly string pawnLabel;
        private readonly string hediffLabel;
        private readonly int currentStageIndex;
        private readonly List<PawnHediffStageSelectionOption> allOptions;

        public PawnHediffStageSelectionWindow(
            Pawn sourcePawn,
            Hediff hediff,
            Action<PawnHediffStageSelectionOption> onStageSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onStageSelected = onStageSelected;
            pawnLabel = sourcePawn.LabelShortCap;
            hediffLabel = PawnCurrentHediffSelectionWindow.GetHediffDisplayLabel(hediff);
            currentStageIndex = hediff.CurStageIndex;
            allOptions = BuildOptions(hediff);
        }

        protected override string TitleKey => "CheatMenu.PawnSetHediffStage.StageWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnSetHediffStage.StageWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnSetHediffStage.StageWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnSetHediffStage.StageWindow.SelectButton";

        protected override IReadOnlyList<PawnHediffStageSelectionOption> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(pawnLabel, hediffLabel);
        }

        protected override void DrawItemInfo(Rect rect, PawnHediffStageSelectionOption option)
        {
            string rowLabel = "CheatMenu.PawnSetHediffStage.StageWindow.RowLabel".Translate(
                option.Index,
                GetStageLabelText(option.Stage));

            if (option.Index == currentStageIndex)
            {
                rowLabel += "CheatMenu.PawnSetHediffStage.StageWindow.CurrentSuffix".Translate();
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), rowLabel);

            string infoLine = "CheatMenu.PawnSetHediffStage.StageWindow.InfoLine".Translate(
                option.MinSeverity.ToString("0.###"),
                BuildFlagsString(option.Stage));

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnHediffStageSelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string indexStr = option.Index.ToString();
            string stageLabel = GetStageLabelText(option.Stage).ToLowerInvariant();
            string overrideLabel = (option.Stage.overrideLabel ?? string.Empty).ToLowerInvariant();
            string severity = option.MinSeverity.ToString("0.###");

            return indexStr.Contains(needle)
                || stageLabel.Contains(needle)
                || overrideLabel.Contains(needle)
                || severity.Contains(needle);
        }

        protected override void OnItemSelected(PawnHediffStageSelectionOption option)
        {
            Close();
            onStageSelected?.Invoke(option);
        }

        private static List<PawnHediffStageSelectionOption> BuildOptions(Hediff hediff)
        {
            List<PawnHediffStageSelectionOption> result = new List<PawnHediffStageSelectionOption>();
            if (hediff.def.stages == null)
            {
                return result;
            }

            for (int i = 0; i < hediff.def.stages.Count; i++)
            {
                result.Add(new PawnHediffStageSelectionOption(i, hediff.def.stages[i]));
            }

            return result;
        }

        private static string GetStageLabelText(HediffStage stage)
        {
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

        private static string BuildFlagsString(HediffStage stage)
        {
            StringBuilder sb = new StringBuilder();
            if (stage.lifeThreatening)
            {
                sb.Append(" | lifeThreatening");
            }
            if (stage.becomeVisible)
            {
                sb.Append(" | becomeVisible");
            }
            if (stage.destroyPart)
            {
                sb.Append(" | destroyPart");
            }
            return sb.ToString();
        }
    }
}
