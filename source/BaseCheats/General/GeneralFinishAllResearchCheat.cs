using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterFinishAllResearch()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralFinishAllResearch",
                "CheatMenu.General.FinishAllResearch.Label",
                "CheatMenu.General.FinishAllResearch.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(FinishAllResearch));
        }

        private static void FinishAllResearch(CheatExecutionContext context)
        {
            Find.ResearchManager.DebugSetAllProjectsFinished();
            Find.EntityCodex.debug_UnhideAllResearch = true;

            CheatMessageService.Message(
                "CheatMenu.General.FinishAllResearch.Message.Result".Translate(),
                MessageTypeDefOf.TaskCompletion,
                false);
        }
    }
}

