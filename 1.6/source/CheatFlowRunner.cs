using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Executes cheat flows step-by-step. Each step decides when the next step starts.
    /// This allows async chains like Window -> Tool -> Action.
    /// </summary>
    public static class CheatFlowRunner
    {
        public static void Execute(CheatDefinition cheat)
        {
            if (cheat == null)
            {
                return;
            }

            CheatExecutionContext context = new CheatExecutionContext(cheat);
            ExecuteStep(cheat, context, 0);
        }

        private static void ExecuteStep(CheatDefinition cheat, CheatExecutionContext context, int stepIndex)
        {
            if (stepIndex >= cheat.Steps.Count)
            {
                return;
            }

            try
            {
                cheat.Steps[stepIndex].Execute(cheat, context, delegate
                {
                    ExecuteStep(cheat, context, stepIndex + 1);
                });
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to execute cheat '" + cheat.Id + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate(cheat.GetLabel()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
