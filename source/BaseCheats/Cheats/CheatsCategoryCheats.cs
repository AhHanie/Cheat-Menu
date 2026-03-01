using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class CheatsCategoryCheats
    {
        public static void Register()
        {
            RegisterEditStatOffsets();
        }

        private static TargetingParameters CreateCellTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetItems = false
            };
        }
    }
}
