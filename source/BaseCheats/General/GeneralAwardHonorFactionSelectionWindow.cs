using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Cheat_Menu
{
    public sealed class GeneralAwardHonorFactionSelectionWindow : Window
    {
        private const float RowHeight = 40f;
        private const float RowSpacing = 4f;
        private const float SelectButtonWidth = 96f;

        private readonly Action<Faction> onFactionSelected;
        private readonly List<Faction> factions;
        private readonly SearchableTableRenderer<Faction> tableRenderer =
            new SearchableTableRenderer<Faction>(RowHeight, RowSpacing);

        public GeneralAwardHonorFactionSelectionWindow(List<Faction> factions, Action<Faction> onFactionSelected)
        {
            this.factions = factions ?? new List<Faction>();
            this.onFactionSelected = onFactionSelected;

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(620f, 460f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.GeneralAwardHonor.FactionWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Widgets.Label(
                new Rect(inRect.x, inRect.y + 40f, inRect.width, 28f),
                "CheatMenu.GeneralAwardHonor.FactionWindow.Description".Translate());

            Rect listRect = new Rect(
                inRect.x,
                inRect.y + 74f,
                inRect.width,
                inRect.height - 74f);

            tableRenderer.Draw(
                listRect,
                factions,
                _ => true,
                DrawFactionRow,
                rect => Widgets.Label(rect, "CheatMenu.GeneralAwardHonor.FactionWindow.NoFactions".Translate()));
        }

        private void DrawFactionRow(Rect rowRect, Faction faction, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect infoRect = new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - SelectButtonWidth - 24f, rowRect.height);
            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 4f, SelectButtonWidth, rowRect.height - 8f);

            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(infoRect, GetFactionDisplayName(faction));
            Text.Anchor = previousAnchor;
            if (Widgets.ButtonText(buttonRect, "CheatMenu.GeneralAwardHonor.FactionWindow.SelectButton".Translate()))
            {
                SelectFaction(faction);
            }

            if (Widgets.ButtonInvisible(infoRect))
            {
                SelectFaction(faction);
            }
        }

        private void SelectFaction(Faction faction)
        {
            Close();
            onFactionSelected?.Invoke(faction);
        }

        private static string GetFactionDisplayName(Faction faction)
        {
            if (faction != null && !faction.Name.NullOrEmpty())
            {
                return faction.Name;
            }

            if (faction?.def?.label != null)
            {
                return faction.def.label;
            }

            return faction?.def?.defName ?? "Unknown faction";
        }
    }
}
