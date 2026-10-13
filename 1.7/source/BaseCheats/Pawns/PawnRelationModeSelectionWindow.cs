using System;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnRelationModeSelectionWindow : Window
    {
        private readonly Pawn pawn;
        private readonly Action<bool> onModeSelected;

        public PawnRelationModeSelectionWindow(Pawn pawn, Action<bool> onModeSelected)
        {
            this.pawn = pawn;
            this.onModeSelected = onModeSelected;

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(500f, 220f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnRelation.ModeWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Widgets.Label(
                new Rect(inRect.x, inRect.y + 40f, inRect.width, 44f),
                "CheatMenu.PawnRelation.ModeWindow.Description".Translate(pawn.LabelShortCap));

            float buttonWidth = 160f;
            float buttonHeight = 42f;
            float spacing = 14f;
            float totalWidth = buttonWidth * 2f + spacing;
            float startX = inRect.x + ((inRect.width - totalWidth) * 0.5f);
            float y = inRect.yMax - buttonHeight - 12f;

            Rect addRect = new Rect(startX, y, buttonWidth, buttonHeight);
            Rect removeRect = new Rect(addRect.xMax + spacing, y, buttonWidth, buttonHeight);

            if (Widgets.ButtonText(addRect, "CheatMenu.PawnRelation.ModeWindow.AddButton".Translate()))
            {
                Close();
                onModeSelected?.Invoke(true);
            }

            bool canRemove = pawn.relations.DirectRelations.Count > 0;
            if (canRemove)
            {
                if (Widgets.ButtonText(removeRect, "CheatMenu.PawnRelation.ModeWindow.RemoveButton".Translate()))
                {
                    Close();
                    onModeSelected?.Invoke(false);
                }
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.DrawBoxSolid(removeRect, Widgets.MenuSectionBGFillColor);
                GUI.color = Color.white;

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(removeRect, "CheatMenu.PawnRelation.ModeWindow.NoRelations".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }
    }
}
