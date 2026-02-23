using System;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class SpawnThingStackCountWindow : Window
    {
        private readonly ThingDef thingDef;
        private readonly Action<int> onConfirm;

        private readonly int maxStackCount;
        private int stackCount;
        private string stackCountBuffer;

        public SpawnThingStackCountWindow(ThingDef thingDef, int initialCount, Action<int> onConfirm)
        {
            this.thingDef = thingDef;
            this.onConfirm = onConfirm;

            maxStackCount = Mathf.Max(1, thingDef.stackLimit);
            stackCount = Mathf.Clamp(initialCount, 1, maxStackCount);
            stackCountBuffer = stackCount.ToString();

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
            Widgets.Label(inRect.TopPart(0.2f), "CheatMenu.SpawnThing.StackWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Rect descRect = new Rect(inRect.x, inRect.y + 44f, inRect.width, 30f);
            Widgets.Label(descRect, "CheatMenu.SpawnThing.StackWindow.Description".Translate(thingDef.LabelCap, maxStackCount));

            Rect controlsRect = new Rect(inRect.x, descRect.yMax + 8f, inRect.width, 32f);
            DrawCountControls(controlsRect);

            Rect sliderRect = new Rect(inRect.x, controlsRect.yMax + 10f, inRect.width, 30f);
            stackCount = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, stackCount, 1f, maxStackCount));

            Rect sliderValueRect = new Rect(inRect.x, sliderRect.yMax + 2f, inRect.width, 22f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(sliderValueRect, "CheatMenu.SpawnThing.StackWindow.CurrentValue".Translate(stackCount, maxStackCount));
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
                onConfirm?.Invoke(Mathf.Clamp(stackCount, 1, maxStackCount));
            }

            if (Widgets.ButtonText(cancelRect, "CheatMenu.Button.Cancel".Translate()))
            {
                Close();
            }
        }

        private void DrawCountControls(Rect rect)
        {
            float labelWidth = 78f;
            float buttonWidth = 44f;
            float fieldWidth = 84f;
            float spacing = 6f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Widgets.Label(labelRect, "CheatMenu.SpawnThing.StackWindow.CountLabel".Translate());

            float x = labelRect.xMax + 8f;
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-100", -100, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-10", -10, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "-1", -1, spacing);

            Rect countFieldRect = new Rect(x, rect.y, fieldWidth, rect.height);
            Widgets.TextFieldNumeric(countFieldRect, ref stackCount, ref stackCountBuffer, 1, maxStackCount);
            stackCount = Mathf.Clamp(stackCount, 1, maxStackCount);
            x = countFieldRect.xMax + spacing;

            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+1", 1, spacing);
            x = DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+10", 10, spacing);
            DrawAdjustButton(x, rect.y, buttonWidth, rect.height, "+100", 100, spacing);
        }

        private float DrawAdjustButton(float x, float y, float width, float height, string label, int delta, float spacing)
        {
            if (Widgets.ButtonText(new Rect(x, y, width, height), label))
            {
                stackCount = Mathf.Clamp(stackCount + delta, 1, maxStackCount);
                stackCountBuffer = stackCount.ToString();
            }

            return x + width + spacing;
        }
    }
}
