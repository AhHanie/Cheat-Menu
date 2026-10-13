using System;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnResistanceSelectionWindow : Window
    {
        private readonly Action<int> onConfirm;
        private readonly string titleKey;
        private readonly string descriptionKey;
        private readonly int minAmount;
        private readonly int maxAmount;

        private int amount;
        private string amountBuffer;

        public PawnResistanceSelectionWindow(
            string titleKey,
            string descriptionKey,
            int initialAmount,
            int minAmount,
            int maxAmount,
            Action<int> onConfirm)
        {
            this.titleKey = titleKey;
            this.descriptionKey = descriptionKey;
            this.onConfirm = onConfirm;
            this.minAmount = Mathf.Max(1, minAmount);
            this.maxAmount = Mathf.Max(this.minAmount, maxAmount);

            amount = Mathf.Clamp(initialAmount, this.minAmount, this.maxAmount);
            amountBuffer = amount.ToString();

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
            Widgets.Label(descriptionRect, descriptionKey.Translate(minAmount, maxAmount));

            Rect controlsRect = new Rect(inRect.x, inRect.y + 82f, inRect.width, 32f);
            DrawAmountControls(controlsRect);

            Rect sliderRect = new Rect(inRect.x, controlsRect.yMax + 10f, inRect.width, 30f);
            amount = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, amount, minAmount, maxAmount));

            Rect sliderValueRect = new Rect(inRect.x, sliderRect.yMax + 2f, inRect.width, 22f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(sliderValueRect, "CheatMenu.PawnResistance.Window.CurrentValue".Translate(amount));
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
                onConfirm?.Invoke(Mathf.Clamp(amount, minAmount, maxAmount));
            }

            if (Widgets.ButtonText(cancelRect, "CheatMenu.Button.Cancel".Translate()))
            {
                Close();
            }
        }

        private void DrawAmountControls(Rect rect)
        {
            float labelWidth = 78f;
            float buttonWidth = 44f;
            float fieldWidth = 84f;
            float spacing = 6f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Widgets.Label(labelRect, "CheatMenu.PawnResistance.Window.AmountLabel".Translate());

            float x = labelRect.xMax + 8f;
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-25", -25, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-10", -10, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-1", -1, spacing);

            Rect amountFieldRect = new Rect(x, rect.y, fieldWidth, rect.height);
            Widgets.TextFieldNumeric(amountFieldRect, ref amount, ref amountBuffer, minAmount, maxAmount);
            amount = Mathf.Clamp(amount, minAmount, maxAmount);
            x = amountFieldRect.xMax + spacing;

            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+1", 1, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+10", 10, spacing);
            DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+25", 25, spacing);
        }

        private float DrawAdjustButton(float x, float y, float width, float height, string label, int delta, float spacing)
        {
            if (Widgets.ButtonText(new Rect(x, y, width, height), label))
            {
                amount = Mathf.Clamp(amount + delta, minAmount, maxAmount);
                amountBuffer = amount.ToString();
            }

            return x + width + spacing;
        }
    }
}
