using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class QuestPopulationOption
    {
        public QuestPopulationOption(int population, bool canRunNow)
        {
            Population = population;
            CanRunNow = canRunNow;
        }

        public int Population { get; }

        public bool CanRunNow { get; }
    }

    public class QuestPopulationSelectionWindow : Window
    {
        private const float RowHeight = 38f;
        private const float RowSpacing = 4f;

        private readonly QuestScriptDef scriptDef;
        private readonly List<QuestPopulationOption> populationOptions;
        private readonly Action<int> onPopulationSelected;

        private Vector2 scrollPosition;

        public QuestPopulationSelectionWindow(QuestScriptDef scriptDef, List<QuestPopulationOption> populationOptions, Action<int> onPopulationSelected)
        {
            this.scriptDef = scriptDef;
            this.populationOptions = populationOptions;
            this.onPopulationSelected = onPopulationSelected;

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
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.Quests.PopulationWindow.Title".Translate());

            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 28f, inRect.width, 24f),
                "CheatMenu.Quests.PopulationWindow.Subtitle".Translate(scriptDef?.LabelCap ?? scriptDef?.defName ?? string.Empty));

            Rect listRect = new Rect(inRect.x, inRect.y + 56f, inRect.width, inRect.height - 56f);
            DrawPopulationList(listRect);
        }

        private void DrawPopulationList(Rect outRect)
        {
            float viewHeight = populationOptions.Count * (RowHeight + RowSpacing);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = 0f;
            for (int i = 0; i < populationOptions.Count; i++)
            {
                QuestPopulationOption option = populationOptions[i];
                Rect rowRect = new Rect(viewRect.x, y, viewRect.width, RowHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawAltRect(rowRect);
                }

                Widgets.DrawHighlightIfMouseover(rowRect);
                string suffix = option.CanRunNow
                    ? string.Empty
                    : "CheatMenu.Quests.PopulationWindow.NotNowSuffix".Translate().ToString();
                if (Widgets.ButtonText(rowRect, "CheatMenu.Quests.PopulationWindow.PopulationButton".Translate(option.Population.ToString(), suffix)))
                {
                    SelectPopulation(option.Population);
                }

                y += RowHeight + RowSpacing;
            }

            Widgets.EndScrollView();
        }

        private void SelectPopulation(int population)
        {
            Close();
            onPopulationSelected?.Invoke(population);
        }
    }
}
