using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterResetResearchTree()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralResetResearchTree",
                "CheatMenu.General.ResetResearchTree.Label",
                "CheatMenu.General.ResetResearchTree.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(ConfirmResetResearchTree)
                    .AddAction(ResetResearchTree));
        }

        private static void ConfirmResetResearchTree(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "CheatMenu.General.ResetResearchTree.Confirmation".Translate(),
                continueFlow,
                destructive: true));
        }

        private static void ResetResearchTree(CheatExecutionContext context)
        {
            ResearchManager researchManager = Find.ResearchManager;
            bool gravEngineInspected = researchManager.gravEngineInspected;

            researchManager.ResetAllProgress();
            researchManager.gravEngineInspected = gravEngineInspected;

            if (ModsConfig.AnomalyActive)
            {
                foreach (ResearchManager.KnowledgeCategoryProject knowledgeProject in researchManager.CurrentAnomalyKnowledgeProjects)
                {
                    knowledgeProject.project = null;
                }

                Find.EntityCodex.debug_UnhideAllResearch = false;
            }

            ResearchUtility.ApplyPlayerStartingResearch();

            CheatMessageService.Message(
                "CheatMenu.General.ResetResearchTree.Message.Result".Translate(),
                MessageTypeDefOf.TaskCompletion,
                false);
        }
    }
}
