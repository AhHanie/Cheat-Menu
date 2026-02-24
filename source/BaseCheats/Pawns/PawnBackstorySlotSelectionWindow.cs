using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnBackstorySlotSelectionWindow : Window
    {
        private readonly Action<BackstorySlot> onSlotSelected;

        public PawnBackstorySlotSelectionWindow(Action<BackstorySlot> onSlotSelected)
        {
            this.onSlotSelected = onSlotSelected;

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(540f, 250f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnSetBackstory.SlotWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Widgets.Label(
                new Rect(inRect.x, inRect.y + 44f, inRect.width, 48f),
                "CheatMenu.PawnSetBackstory.SlotWindow.Description".Translate());

            float buttonTop = inRect.y + 108f;
            float buttonWidth = (inRect.width - 8f) / 2f;
            Rect adulthoodButtonRect = new Rect(inRect.x, buttonTop, buttonWidth, 40f);
            Rect childhoodButtonRect = new Rect(adulthoodButtonRect.xMax + 8f, buttonTop, buttonWidth, 40f);

            if (Widgets.ButtonText(adulthoodButtonRect, "CheatMenu.PawnSetBackstory.Slot.Adulthood".Translate()))
            {
                SelectSlot(BackstorySlot.Adulthood);
            }

            if (Widgets.ButtonText(childhoodButtonRect, "CheatMenu.PawnSetBackstory.Slot.Childhood".Translate()))
            {
                SelectSlot(BackstorySlot.Childhood);
            }
        }

        private void SelectSlot(BackstorySlot slot)
        {
            Close();
            onSlotSelected?.Invoke(slot);
        }
    }
}
