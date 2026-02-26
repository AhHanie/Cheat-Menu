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
                "CheatMenu.General.ToggleGodMode.Label",
                "CheatMenu.General.ToggleGodMode.Description",
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
                    "CheatMenu.General.ToggleGodMode.Message.Enabled".Translate(),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            else
            {
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                CheatMessageService.Message(
                    "CheatMenu.General.ToggleGodMode.Message.Disabled".Translate(),
                    MessageTypeDefOf.NeutralEvent,
                    false);
            }
        }
    }
}

