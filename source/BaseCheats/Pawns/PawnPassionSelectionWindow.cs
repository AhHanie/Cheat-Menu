using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class PawnPassionSelectionOption
    {
        public PawnPassionSelectionOption(Passion passion, string displayLabel)
        {
            Passion = passion;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public Passion Passion { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnPassionSelectionWindow : Window
    {
        private const float RowHeight = 40f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 96f;

        private readonly Action<PawnPassionSelectionOption> onPassionSelected;
        private readonly List<PawnPassionSelectionOption> allOptions;
        private readonly SearchableTableRenderer<PawnPassionSelectionOption> tableRenderer =
            new SearchableTableRenderer<PawnPassionSelectionOption>(RowHeight, RowSpacing);

        public PawnPassionSelectionWindow(Action<PawnPassionSelectionOption> onPassionSelected)
        {
            this.onPassionSelected = onPassionSelected;
            allOptions = BuildPassions();

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(420f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.PawnSetPassion.PassionWindow.Title".Translate());
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
                DrawPassionRow,
                rect => Widgets.Label(rect, "CheatMenu.PawnSetPassion.PassionWindow.NoOptions".Translate()));
        }

        private void DrawPassionRow(Rect rowRect, PawnPassionSelectionOption option, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - SelectButtonWidth - 24f, rowRect.height);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 4f, SelectButtonWidth, rowRect.height - 8f);

            Widgets.Label(infoRect, option.DisplayLabel);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.PawnSetPassion.PassionWindow.SelectButton".Translate()))
            {
                SelectPassion(option);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectPassion(option);
            }
        }

        private void SelectPassion(PawnPassionSelectionOption option)
        {
            Close();
            onPassionSelected?.Invoke(option);
        }

        private static List<PawnPassionSelectionOption> BuildPassions()
        {
            return Enum.GetValues(typeof(Passion))
                .Cast<Passion>()
                .OrderBy(passion => (int)passion)
                .Select(passion => new PawnPassionSelectionOption(passion, GetPassionLabel(passion)))
                .ToList();
        }

        private static string GetPassionLabel(Passion passion)
        {
            switch (passion)
            {
                case Passion.None:
                    return "CheatMenu.PawnSetPassion.Passion.None".Translate();
                case Passion.Minor:
                    return "CheatMenu.PawnSetPassion.Passion.Minor".Translate();
                case Passion.Major:
                    return "CheatMenu.PawnSetPassion.Passion.Major".Translate();
                default:
                    return passion.ToString();
            }
        }
    }
}
