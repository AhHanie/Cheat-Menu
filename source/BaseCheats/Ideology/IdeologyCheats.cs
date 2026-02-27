using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class IdeologyCheats
    {
        public static void Register()
        {
            RegisterSpawnRelic();
        }

        private static RimWorld.TargetingParameters CreateCellTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new RimWorld.TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetItems = false
            };
        }
    }
}
