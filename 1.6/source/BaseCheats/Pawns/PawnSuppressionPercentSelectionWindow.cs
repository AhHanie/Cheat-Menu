using System;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnSuppressionPercentSelectionWindow : Window
    {
        private readonly Action<int> onConfirm;
        private readonly string titleKey;
        private readonly string descriptionKey;
        private readonly int minPercent;
        private readonly int maxPercent;

        private int percent;
        private string percentBuffer;

        public PawnSuppressionPercentSelectionWindow(
            string titleKey,
            string descriptionKey,
            int initialPercent,
            int minPercent,
            int maxPercent,
            Action<int> onConfirm)
        {
            this.titleKey = titleKey;
            this.descriptionKey = descriptionKey;
            this.onConfirm = onConfirm;
            this.minPercent = Mathf.Max(1, minPercent);
            this.maxPercent = Mathf.Max(this.minPercent, maxPercent);

            percent = Mathf.Clamp(initialPercent, this.minPercent, this.maxPercent);
            percentBuffer = percent.ToString();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(520f, 250f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(inRect.TopPart(0.2f), titleKey.Translate());
            Text.Font = GameFont.Small;

            Rect descriptionRect = new Rect(inRect.x, inRect.y + 44f, inRect.width, 30f);
            Widgets.Label(descriptionRect, descriptionKey.Translate(minPercent, maxPercent));

            Rect controlsRect = new Rect(inRect.x, inRect.y + 82f, inRect.width, 32f);
            DrawPercentControls(controlsRect);

            Rect sliderRect = new Rect(inRect.x, controlsRect.yMax + 10f, inRect.width, 30f);
            percent = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, percent, minPercent, maxPercent));

            Rect sliderValueRect = new Rect(inRect.x, sliderRect.yMax + 2f, inRect.width, 22f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(sliderValueRect, "CheatMenu.PawnSuppression.Window.CurrentValue".Translate(percent));
            Text.Anchor = TextAnchor.UpperLeft;

            Rect buttonsRect = new Rect(inRect.x, inRect.yMax - 38f, inRect.width, 38f);
            DrawButtons(buttonsRect);
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
                onConfirm?.Invoke(Mathf.Clamp(percent, minPercent, maxPercent));
            }

            if (Widgets.ButtonText(cancelRect, "CheatMenu.Button.Cancel".Translate()))
            {
                Close();
            }
        }

        private void DrawPercentControls(Rect rect)
        {
            float labelWidth = 78f;
            float buttonWidth = 44f;
            float fieldWidth = 84f;
            float spacing = 6f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Widgets.Label(labelRect, "CheatMenu.PawnSuppression.Window.PercentLabel".Translate());

            float x = labelRect.xMax + 8f;
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-25", -25, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-10", -10, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-1", -1, spacing);

            Rect percentFieldRect = new Rect(x, rect.y, fieldWidth, rect.height);
            Widgets.TextFieldNumeric(percentFieldRect, ref percent, ref percentBuffer, minPercent, maxPercent);
            percent = Mathf.Clamp(percent, minPercent, maxPercent);
            x = percentFieldRect.xMax + spacing;

            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+1", 1, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+10", 10, spacing);
            DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+25", 25, spacing);
        }

        private float DrawAdjustButton(float x, float y, float width, float height, string label, int delta, float spacing)
        {
            if (Widgets.ButtonText(new Rect(x, y, width, height), label))
            {
                percent = Mathf.Clamp(percent + delta, minPercent, maxPercent);
                percentBuffer = percent.ToString();
            }

            return x + width + spacing;
        }
    }
}
