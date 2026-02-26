using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Cheat_Menu
{
    public static class QuestGenerateQuestCheat
    {
        private const int GenerateQuestCount = 1;

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.QuestGenerateQuest",
                "CheatMenu.Cheat.QuestGenerateQuest.Label",
                "CheatMenu.Cheat.QuestGenerateQuest.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Quests")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(OpenGenerateQuestMenu));
        }

        private static void OpenGenerateQuestMenu(CheatExecutionContext context)
        {
            Find.WindowStack.Add(new QuestScriptSelectionWindow(BuildQuestSelectionOptions(), SelectQuestSelectionOption));
        }

        private static List<QuestScriptSelectionOption> BuildQuestSelectionOptions()
        {
            List<QuestScriptSelectionOption> options = new List<QuestScriptSelectionOption>
            {
                new QuestScriptSelectionOption("CheatMenu.Quests.Window.NaturalRandom".Translate().ToString(), null, true)
            };

            foreach (QuestScriptDef scriptDef in DefDatabase<QuestScriptDef>.AllDefs.Where(x => x.IsRootAny).OrderBy(x => x.defName))
            {
                options.Add(new QuestScriptSelectionOption(scriptDef.defName, scriptDef, false));
            }

            return options;
        }

        private static void SelectQuestSelectionOption(QuestScriptSelectionOption option)
        {
            if (option.IsNaturalRandom)
            {
                Slate slate = CreateBaseSlate();
                QuestScriptDef script = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(slate.Get("points", 0f), Find.CurrentMap);
                GetQuest(script, slate, GenerateQuestCount, logDescOnly: false);
                return;
            }

            QuestScriptDef questScript = option.ScriptDef;
            Slate questSlate = CreateBaseSlate();
            if (questScript.affectedByPoints && questScript.affectedByPopulation)
            {
                OpenPointsSelection(questScript, questSlate, generate: false, GenerateQuestCount, logDescOnly: false);
                return;
            }

            if (questScript.affectedByPoints)
            {
                OpenPointsSelection(questScript, questSlate, generate: true, GenerateQuestCount, logDescOnly: false);
                return;
            }

            if (questScript.affectedByPopulation)
            {
                OpenPopulationSelection(questScript, questSlate, generate: true, GenerateQuestCount, logDescOnly: false);
                return;
            }

            GetQuest(questScript, questSlate, GenerateQuestCount, logDescOnly: false);
        }

        private static Slate CreateBaseSlate()
        {
            Slate slate = new Slate();
            slate.Set("discoveryMethod", "QuestDiscoveredFromDebug".Translate());
            return slate;
        }

        private static void OpenPointsSelection(QuestScriptDef script, Slate slate, bool generate, int count, bool logDescOnly)
        {
            List<QuestPointsOption> pointsOptions = BuildPointsOptions(script, slate);
            Find.WindowStack.Add(new QuestPointsSelectionWindow(script, pointsOptions, delegate (float selectedPoints)
            {
                Slate localSlate = slate.DeepCopy();
                GetSlateValuesForPoints(script, localSlate, selectedPoints);

                if (script.affectedByPopulation && !generate)
                {
                    OpenPopulationSelection(script, localSlate, generate: true, count, logDescOnly);
                    return;
                }

                if (generate)
                {
                    GetQuest(script, localSlate, count, logDescOnly);
                }
            }));
        }

        private static List<QuestPointsOption> BuildPointsOptions(QuestScriptDef script, Slate slate)
        {
            List<QuestPointsOption> options = new List<QuestPointsOption>();
            foreach (float points in DebugActionsUtility.PointsOptions(extended: false))
            {
                Slate optionSlate = slate.DeepCopy();
                GetSlateValuesForPoints(script, optionSlate, points);

                IIncidentTarget currentMap = Find.CurrentMap;
                bool canRunNow = script.CanRun(optionSlate, currentMap ?? Find.World);
                options.Add(new QuestPointsOption(points, canRunNow));
            }

            return options;
        }

        private static void OpenPopulationSelection(QuestScriptDef script, Slate slate, bool generate, int count, bool logDescOnly)
        {
            List<QuestPopulationOption> populationOptions = BuildPopulationOptions(script, slate);
            Find.WindowStack.Add(new QuestPopulationSelectionWindow(script, populationOptions, delegate (int selectedPopulation)
            {
                Slate localSlate = slate.DeepCopy();
                localSlate.Set("population", selectedPopulation);
                if (generate)
                {
                    GetQuest(script, localSlate, count, logDescOnly);
                }
            }));
        }

        private static List<QuestPopulationOption> BuildPopulationOptions(QuestScriptDef script, Slate slate)
        {
            List<QuestPopulationOption> options = new List<QuestPopulationOption>();
            foreach (int population in PopulationOptions())
            {
                Slate optionSlate = slate.DeepCopy();
                optionSlate.Set("population", population);

                IIncidentTarget currentMap = Find.CurrentMap;
                bool canRunNow = script.CanRun(optionSlate, currentMap ?? Find.World);
                options.Add(new QuestPopulationOption(population, canRunNow));
            }

            return options;
        }

        private static IEnumerable<int> PopulationOptions()
        {
            for (int i = 1; i <= 20; i++)
            {
                yield return i;
            }

            for (int i = 30; i <= 50; i += 10)
            {
                yield return i;
            }
        }

        private static void GetQuest(QuestScriptDef script, Slate slate, int count, bool logDescOnly)
        {
            int failedCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (script.IsRootDecree)
                {
                    Pawn pawn = slate.Get<Pawn>("asker");
                    if (pawn.royalty.AllTitlesForReading.NullOrEmpty() && Faction.OfEmpire != null)
                    {
                        pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Knight, grantRewards: false);
                        Messages.Message("DEV: Gave " + RoyalTitleDefOf.Knight.label + " title to " + pawn.LabelCap, pawn, MessageTypeDefOf.NeutralEvent, historical: false);
                    }

                    Find.CurrentMap.StoryState.RecordDecreeFired(script);
                }

                IIncidentTarget currentMap = Find.CurrentMap;
                if (!script.CanRun(slate, currentMap ?? Find.World))
                {
                    if (count == 1 && !script.affectedByPoints && !script.affectedByPopulation)
                    {
                        Messages.Message("DEV: Failed to generate quest. CanRun returned false.", MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (count > 1)
                    {
                        failedCount++;
                    }
                }
                else if (!logDescOnly)
                {
                    Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, slate);
                    if (!quest.hidden && quest.root.sendAvailableLetter)
                    {
                        QuestUtility.SendLetterQuestAvailable(quest);
                    }
                }
                else
                {
                    Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, slate);
                    string text = quest.name;
                    if (slate.Exists("points"))
                    {
                        text = text + "(" + slate.Get("points", 0f) + " points)";
                    }

                    if (slate.Exists("population"))
                    {
                        text = text + "(" + slate.Get("population", 0) + " population)";
                    }

                    text += "\n--------------\n" + quest.description + "\n--------------";
                    Log.Message(text);
                    Find.QuestManager.Remove(quest);
                }
            }

            if (failedCount != 0)
            {
                Messages.Message("DEV: Generated only " + (count - failedCount) + " quests.", MessageTypeDefOf.RejectInput, historical: false);
            }
        }

        private static void GetSlateValuesForPoints(QuestScriptDef script, Slate slate, float points)
        {
            if (script != null)
            {
                if (script.IsRootDecree)
                {
                    slate.Set("asker", PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.RandomElement());
                }

                if (script == QuestScriptDefOf.LongRangeMineralScannerLump)
                {
                    slate.Set("targetMineableThing", ThingDefOf.Gold);
                    slate.Set("targetMineable", ThingDefOf.MineableGold);
                    slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
                }

                slate.Set("points", points);
            }
        }
    }
}
