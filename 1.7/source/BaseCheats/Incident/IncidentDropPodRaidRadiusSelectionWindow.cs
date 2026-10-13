using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentDropPodRaidRadiusSelectionWindow : Window
    {
        private const float RowHeight = 38f;
        private const float RowSpacing = 4f;

        private readonly Faction faction;
        private readonly float points;
        private readonly List<int> radiusOptions;
        private readonly Action<int> onRadiusSelected;

        private Vector2 scrollPosition;

        public IncidentDropPodRaidRadiusSelectionWindow(Faction faction, float points, List<int> radiusOptions, Action<int> onRadiusSelected)
        {
            this.faction = faction;
            this.points = points;
            this.radiusOptions = radiusOptions;
            this.onRadiusSelected = onRadiusSelected;

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(420f, 640f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Incidents.DropPodRaidRadiusWindow.Title".Translate());

            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 28f, inRect.width, 24f),
                "CheatMenu.Incidents.DropPodRaidRadiusWindow.Subtitle".Translate(faction.Name, points.ToString("F0")));

            Rect listRect = new Rect(inRect.x, inRect.y + 56f, inRect.width, inRect.height - 56f);
            DrawRadiusList(listRect);
        }

        private void DrawRadiusList(Rect outRect)
        {
            float viewHeight = radiusOptions.Count * (RowHeight + RowSpacing);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = 0f;
            for (int i = 0; i < radiusOptions.Count; i++)
            {
                int radius = radiusOptions[i];
                Rect rowRect = new Rect(viewRect.x, y, viewRect.width, RowHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawAltRect(rowRect);
                }

                Widgets.DrawHighlightIfMouseover(rowRect);
                if (Widgets.ButtonText(rowRect, "CheatMenu.Incidents.DropPodRaidRadiusWindow.RadiusButton".Translate(radius.ToString())))
                {
                    SelectRadius(radius);
                }

                y += RowHeight + RowSpacing;
            }

            Widgets.EndScrollView();
        }

        private void SelectRadius(int radius)
        {
            Close();
            onRadiusSelected?.Invoke(radius);
        }
    }
}
