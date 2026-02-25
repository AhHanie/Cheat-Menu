using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class SpawningCheats
    {
        public static void Register()
        {
            SpawnThingCheat.Register();
            SpawnPawnCheat.Register();
        }

        public static TargetingParameters CreateCellTargetingParameters(CheatExecutionContext context)
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

        public static string GetPawnKindCategoryKey(PawnKindDef pawnKindDef)
        {
            if (pawnKindDef.race.race == null)
            {
                return "CheatMenu.SpawnPawn.Category.Other";
            }

            RaceProperties raceProperties = pawnKindDef.race.race;
            if (raceProperties.Humanlike)
            {
                return "CheatMenu.SpawnPawn.Category.Humanlike";
            }

            if (raceProperties.Animal)
            {
                return "CheatMenu.SpawnPawn.Category.Animal";
            }

            if (raceProperties.IsMechanoid)
            {
                return "CheatMenu.SpawnPawn.Category.Mechanoid";
            }

            if (raceProperties.Insect)
            {
                return "CheatMenu.SpawnPawn.Category.Insect";
            }

            return "CheatMenu.SpawnPawn.Category.Other";
        }
    }
}
