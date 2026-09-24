using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(Game), "Dispose")]
    public static class Game_Dispose_Patch
    {
        public static void Postfix()
        {
            VisualTimeOfDayPatchController.Clear();
        }
    }
}
