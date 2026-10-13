using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class QuestPointsOption
    {
        public QuestPointsOption(float points, bool canRunNow)
        {
            Points = points;
            CanRunNow = canRunNow;
        }

        public float Points { get; }

        public bool CanRunNow { get; }
    }

    public class QuestPointsSelectionWindow : Window
    {
        private const float RowHeight = 38f;
        private const float RowSpacing = 4f;

        private readonly QuestScriptDef scriptDef;
        private readonly List<QuestPointsOption> pointOptions;
        private readonly Action<float> onPointsSelected;

        private Vector2 scrollPosition;

        public QuestPointsSelectionWindow(QuestScriptDef scriptDef, List<QuestPointsOption> pointOptions, Action<float> onPointsSelected)
        {
            this.scriptDef = scriptDef;
            this.pointOptions = pointOptions ?? new List<QuestPointsOption>();
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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Quests.PointsWindow.Title".Translate());

            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 28f, inRect.width, 24f),
                "CheatMenu.Quests.PointsWindow.Subtitle".Translate(scriptDef.defName));

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
                QuestPointsOption option = pointOptions[i];
                Rect rowRect = new Rect(viewRect.x, y, viewRect.width, RowHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawAltRect(rowRect);
                }

                Widgets.DrawHighlightIfMouseover(rowRect);
                string suffix = option.CanRunNow
                    ? string.Empty
                    : "CheatMenu.Quests.PointsWindow.NotNowSuffix".Translate().ToString();
                if (Widgets.ButtonText(rowRect, "CheatMenu.Quests.PointsWindow.PointsButton".Translate(option.Points.ToString("F0"), suffix)))
                {
                    SelectPoints(option.Points);
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
