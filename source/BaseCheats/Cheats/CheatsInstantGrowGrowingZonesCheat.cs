using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class CheatsCategoryCheats
    {
        private static void RegisterInstantGrowGrowingZones()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.Cheats.InstantGrowGrowingZones",
                "CheatMenu.Cheats.InstantGrowGrowingZones.Label",
                "CheatMenu.Cheats.InstantGrowGrowingZones.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Cheats")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(InstantGrowGrowingZones));
        }

        private static void InstantGrowGrowingZones(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            int grownPlantCount = 0;
            List<Zone> zones = map.zoneManager.AllZones;
            for (int zoneIndex = 0; zoneIndex < zones.Count; zoneIndex++)
            {
                Zone_Growing growingZone = zones[zoneIndex] as Zone_Growing;
                if (growingZone == null)
                {
                    continue;
                }

                List<IntVec3> cells = growingZone.Cells;
                for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                {
                    Plant plant = cells[cellIndex].GetPlant(map);
                    if (plant == null || plant.def.plant == null)
                    {
                        continue;
                    }

                    if (plant.Growth >= 1f)
                    {
                        continue;
                    }

                    int growthRemaining = (int)((1f - plant.Growth) * plant.def.plant.growDays);
                    plant.Age += growthRemaining;
                    plant.Growth = 1f;
                    grownPlantCount++;
                    map.mapDrawer.MapMeshDirty(cells[cellIndex], MapMeshFlagDefOf.Things);
                }
            }

            

            CheatMessageService.Message(
                "CheatMenu.Cheats.InstantGrowGrowingZones.Message.Result".Translate(grownPlantCount),
                MessageTypeDefOf.TaskCompletion,
                false);
        }
    }
}
