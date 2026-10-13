using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentDropPodRaidPointsSelectionWindow : Window
    {
        private const float RowHeight = 38f;
        private const float RowSpacing = 4f;

        private readonly Faction faction;
        private readonly List<float> pointOptions;
        private readonly Action<float> onPointsSelected;

        private Vector2 scrollPosition;

        public IncidentDropPodRaidPointsSelectionWindow(Faction faction, List<float> pointOptions, Action<float> onPointsSelected)
        {
            this.faction = faction;
            this.pointOptions = pointOptions;
            this.onPointsSelected = onPointsSelected;

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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Incidents.DropPodRaidPointsWindow.Title".Translate());

            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 28f, inRect.width, 24f),
                "CheatMenu.Incidents.DropPodRaidPointsWindow.Subtitle".Translate(faction.Name));

            Rect listRect = new Rect(inRect.x, inRect.y + 56f, inRect.width, inRect.height - 56f);
            DrawPointsList(listRect);
        }

        private void DrawPointsList(Rect outRect)
        {
            float viewHeight = pointOptions.Count * (RowHeight + RowSpacing);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = 0f;
            for (int i = 0; i < pointOptions.Count; i++)
            {
                float points = pointOptions[i];
                Rect rowRect = new Rect(viewRect.x, y, viewRect.width, RowHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawAltRect(rowRect);
                }

                Widgets.DrawHighlightIfMouseover(rowRect);
                if (Widgets.ButtonText(rowRect, "CheatMenu.Incidents.DropPodRaidPointsWindow.PointsButton".Translate(points.ToString("F0"))))
                {
                    SelectPoints(points);
                }

                y += RowHeight + RowSpacing;
            }

            Widgets.EndScrollView();
        }

        private void SelectPoints(float points)
        {
            Close();
            onPointsSelected?.Invoke(points);
        }
    }
}
