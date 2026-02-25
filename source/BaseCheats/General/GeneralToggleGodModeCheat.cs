using RimWorld;
using Verse;
using Verse.Sound;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterToggleGodMode()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralToggleGodMode",
                "CheatMenu.Cheat.GeneralToggleGodMode.Label",
                "CheatMenu.Cheat.GeneralToggleGodMode.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddAction(ToggleGodMode));
        }

        private static void ToggleGodMode(CheatExecutionContext context)
        {
            DebugSettings.godMode = !DebugSettings.godMode;

            if (DebugSettings.godMode)
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                CheatMessageService.Message(
                    "CheatMenu.GeneralToggleGodMode.Message.Enabled".Translate(),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            else
            {
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                CheatMessageService.Message(
                    "CheatMenu.GeneralToggleGodMode.Message.Disabled".Translate(),
                    MessageTypeDefOf.NeutralEvent,
                    false);
            }
        }
    }
}
