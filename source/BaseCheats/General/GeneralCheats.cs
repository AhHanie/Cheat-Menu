using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        public static void Register()
        {
            RegisterDestroy();
            RegisterKill();
            RegisterLightningStrike();
            RegisterLightningStrikeDelayed();
            RegisterChangeWeather();
            RegisterDamage10();
            RegisterDamageX();
            RegisterAddGas();
            RegisterFinishAllResearch();
            RegisterToggleGodMode();
            RegisterToggleLogWindow();
            RegisterForceEnemyFlee();
            RegisterSetQuality();
            RegisterSetFaction();
            RegisterChangeThingStyle();
            RegisterEditRoofRect();
            RegisterFogRect();
            RegisterUnfogRect();
            RegisterAwardHonor();
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

        private static TargetingParameters CreatePawnTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = false,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }
    }
}
