using Verse;

namespace Cheat_Menu
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool SendCheatMessages = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SendCheatMessages, "sendCheatMessages", true);
        }
    }
}
