using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnHediffSeveritySelectionWindow : Window
    {
        private readonly Action<float> onSeveritySelected;
        private readonly HediffDef hediffDef;
        private readonly string pawnLabel;
        private readonly string hediffLabel;
        private readonly float minSeverity;
        private readonly float maxSeverity;
        private readonly float sliderMaxSeverity;

        private float severity;
        private string severityBuffer;

        public PawnHediffSeveritySelectionWindow(Pawn sourcePawn, Hediff hediff, Action<float> onSeveritySelected)
        {
            this.onSeveritySelected = onSeveritySelected;
            hediffDef = hediff.def;
            pawnLabel = sourcePawn.LabelShortCap;
            hediffLabel = PawnCurrentHediffSelectionWindow.GetHediffDisplayLabel(hediff);
            minSeverity = hediff.def.minSeverity;
            maxSeverity = hediff.def.maxSeverity;
            sliderMaxSeverity = ComputeSliderMax(hediff);
            severity = hediff.Severity;
            severityBuffer = severity.ToString("0.###");

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(560f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            string titleText = "CheatMenu.PawnSetHediffSeverity.Window.Title".Translate(pawnLabel, hediffLabel);
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), titleText);
            Text.Font = GameFont.Small;

            string maxDisplay = maxSeverity >= float.MaxValue
                ? "CheatMenu.PawnSetHediffSeverity.Window.UnboundedMax".Translate().ToString()
                : maxSeverity.ToString("0.###");

            string descText = "CheatMenu.PawnSetHediffSeverity.Window.Description".Translate(
                severity.ToString("0.###"),
                minSeverity.ToString("0.###"),
                maxDisplay);
            Widgets.Label(new Rect(inRect.x, inRect.y + 44f, inRect.width, 24f), descText);

            int stageIndex = hediffDef.stages != null && hediffDef.stages.Count > 0
                ? hediffDef.StageAtSeverity(severity)
                : -1;
            HediffStage curStage = stageIndex >= 0 ? hediffDef.stages[stageIndex] : null;
            string stageName = stageIndex >= 0
                ? PawnCurrentHediffSelectionWindow.GetStageLabel(curStage, stageIndex)
                : "CheatMenu.PawnSetHediffSeverity.Window.NoStage".Translate().ToString();
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 72f, inRect.width, 22f),
                "CheatMenu.PawnSetHediffSeverity.Window.StageLine".Translate(stageName));

            DrawSeverityControls(new Rect(inRect.x, inRect.y + 100f, inRect.width, 32f));

            float sliderClampedValue = Mathf.Clamp(severity, minSeverity, sliderMaxSeverity);
            float newSliderValue = Widgets.HorizontalSlider(
                new Rect(inRect.x, inRect.y + 140f, inRect.width, 30f),
                sliderClampedValue,
                minSeverity,
                sliderMaxSeverity);
            if (Mathf.Abs(newSliderValue - sliderClampedValue) > 0.0001f)
            {
                severity = newSliderValue;
                severityBuffer = severity.ToString("0.###");
            }

            DrawButtons(new Rect(inRect.x, inRect.yMax - 38f, inRect.width, 38f));
        }

        private void DrawSeverityControls(Rect rect)
        {
            float labelWidth = 76f;
            float buttonWidth = 48f;
            float fieldWidth = 90f;
            float spacing = 5f;

            Widgets.Label(new Rect(rect.x, rect.y, labelWidth, rect.height),
                "CheatMenu.PawnSetHediffSeverity.Window.SeverityLabel".Translate());

            float x = rect.x + labelWidth + 6f;
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-10", -10f, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-1", -1f, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-0.1", -0.1f, spacing);

            float fieldMax = maxSeverity < float.MaxValue ? maxSeverity : 1E+09f;
            Rect fieldRect = new Rect(x, rect.y, fieldWidth, rect.height);
            Widgets.TextFieldNumeric(fieldRect, ref severity, ref severityBuffer, minSeverity, fieldMax);
            x = fieldRect.xMax + spacing;

            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+0.1", 0.1f, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+1", 1f, spacing);
            DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+10", 10f, spacing);
        }

        private float DrawAdjustButton(float x, float y, float width, float height, string label, float delta, float spacing)
        {
            if (Widgets.ButtonText(new Rect(x, y, width, height), label))
            {
                float newVal = severity + delta;
                newVal = Mathf.Max(newVal, minSeverity);
                if (maxSeverity < float.MaxValue)
                {
                    newVal = Mathf.Min(newVal, maxSeverity);
                }
                severity = newVal;
                severityBuffer = severity.ToString("0.###");
            }
            return x + width + spacing;
        }

        private void DrawButtons(Rect rect)
        {
            float buttonWidth = 140f;
            float spacing = 10f;

            Rect confirmRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
            Rect cancelRect = new Rect(confirmRect.x - spacing - buttonWidth, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(confirmRect, "CheatMenu.Button.Confirm".Translate()))
            {
                Close();
                onSeveritySelected?.Invoke(severity);
            }

            if (Widgets.ButtonText(cancelRect, "CheatMenu.Button.Cancel".Translate()))
            {
                Close();
            }
        }

        private static float ComputeSliderMax(Hediff hediff)
        {
            float max = hediff.def.maxSeverity;
            if (max < float.MaxValue)
            {
                return max;
            }

            float derived = Mathf.Max(1f, hediff.Severity);
            derived = Mathf.Max(derived, hediff.def.initialSeverity);

            if (hediff.def.lethalSeverity > 0f)
            {
                derived = Mathf.Max(derived, hediff.def.lethalSeverity);
            }

            if (hediff.def.stages != null)
            {
                foreach (HediffStage stage in hediff.def.stages)
                {
                    derived = Mathf.Max(derived, stage.minSeverity);
                }
            }

            if (hediff is Hediff_Injury && hediff.Part != null)
            {
                float partMax = hediff.Part.def.GetMaxHealth(hediff.pawn);
                if (partMax > 0f)
                {
                    derived = Mathf.Max(derived, partMax);
                }
            }

            return derived;
        }
    }
}
