using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnSkillLevelSelectionOption
    {
        public PawnSkillLevelSelectionOption(int level)
        {
            Level = level;
        }

        public int Level { get; }
    }

    public sealed class PawnSkillLevelSelectionWindow : Window
    {
        private const float RowHeight = 40f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 96f;

        private readonly Action<PawnSkillLevelSelectionOption> onLevelSelected;
        private readonly List<PawnSkillLevelSelectionOption> allOptions;
        private readonly SearchableTableRenderer<PawnSkillLevelSelectionOption> tableRenderer =
            new SearchableTableRenderer<PawnSkillLevelSelectionOption>(RowHeight, RowSpacing);

        public PawnSkillLevelSelectionWindow(Action<PawnSkillLevelSelectionOption> onLevelSelected)
        {
            this.onLevelSelected = onLevelSelected;
            allOptions = BuildLevels();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(420f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnSetSkill.LevelWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(
                inRect.x,
                inRect.y + 40f,
                inRect.width,
                inRect.height - 40f);

            tableRenderer.Draw(
                listRect,
                allOptions,
                _ => true,
                DrawLevelRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnSetSkill.LevelWindow.NoLevels".Translate()));
        }

        private void DrawLevelRow(Rect rowRect, PawnSkillLevelSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - SelectButtonWidth - 24f, rowRect.height);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 4f, SelectButtonWidth, rowRect.height - 8f);

            Widgets.Label(infoRect, "CheatMenu.PawnSetSkill.LevelWindow.LevelLabel".Translate(option.Level));
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnSetSkill.LevelWindow.SelectButton".Translate()))
            {
                SelectLevel(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectLevel(option);
            }
        }

        private void SelectLevel(PawnSkillLevelSelectionOption option)
        {
            Close();
            onLevelSelected?.Invoke(option);
        }

        private static List<PawnSkillLevelSelectionOption> BuildLevels()
        {
            List<PawnSkillLevelSelectionOption> result = new List<PawnSkillLevelSelectionOption>();
            for (int i = 0; i <= 20; i++)
            {
                result.Add(new PawnSkillLevelSelectionOption(i));
            }

            return result;
        }
    }
}
