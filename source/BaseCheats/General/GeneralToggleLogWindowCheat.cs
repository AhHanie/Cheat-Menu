using RimWorld;
using Verse;
using LudeonTK;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterToggleLogWindow()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralToggleLogWindow",
                "CheatMenu.Cheat.GeneralToggleLogWindow.Label",
                "CheatMenu.Cheat.GeneralToggleLogWindow.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddAction(ToggleLogWindow));
        }

        private static void ToggleLogWindow(CheatExecutionContext context)
        {
            if (!Find.WindowStack.TryRemove(typeof(EditWindow_Log)))
            {
                Find.WindowStack.Add(new EditWindow_Log());
                CheatMessageService.Message(
                    "CheatMenu.GeneralToggleLogWindow.Message.Opened".Translate(),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.GeneralToggleLogWindow.Message.Closed".Translate(),
                MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
