using RimWorld;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Centralized in-game messaging for cheats.
    /// Respects mod settings before sending messages to the player.
    /// </summary>
    public static class CheatMessageService
    {
        public static void Message(string text, MessageTypeDef messageType, bool historical = false)
        {
            if (!ModSettings.SendCheatMessages)
            {
                return;
            }

            Messages.Message(text, messageType, historical);
        }
    }
}
