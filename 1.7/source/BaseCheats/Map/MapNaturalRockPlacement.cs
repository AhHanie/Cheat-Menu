using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public readonly struct MapNaturalRockPlacementResult
    {
        public MapNaturalRockPlacementResult(int placedCount, int skippedCount)
        {
            PlacedCount = placedCount;
            SkippedCount = skippedCount;
        }

        public int PlacedCount { get; }

        public int SkippedCount { get; }
    }

    public static class MapNaturalRockPlacement
    {
        public static RoofDef GetRoofDef(MapNaturalRockRoofMode roofMode)
        {
            switch (roofMode)
            {
                case MapNaturalRockRoofMode.OverheadMountain:
                    return RoofDefOf.RoofRockThick;
                case MapNaturalRockRoofMode.ThinRockRoof:
                    return RoofDefOf.RoofRockThin;
                default:
                    return null;
            }
        }

        /// <param name="roofDef">Roof to apply on placed cells. Null leaves roofs untouched.</param>
        public static MapNaturalRockPlacementResult Place(Map map, IEnumerable<IntVec3> cells, ThingDef rockDef, RoofDef roofDef)
        {
            int placedCount = 0;
            int skippedCount = 0;

            foreach (IntVec3 cell in cells)
            {
                if (TryPlace(map, cell, rockDef, roofDef))
                {
                    placedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            return new MapNaturalRockPlacementResult(placedCount, skippedCount);
        }

        private static bool TryPlace(Map map, IntVec3 cell, ThingDef rockDef, RoofDef roofDef)
        {
            if (!cell.InBounds(map))
            {
                return false;
            }

            TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
            if (!terrain.natural || !terrain.supportsRock || terrain.IsWater || terrain.IsRoad)
            {
                return false;
            }

            if (map.edificeGrid[cell] != null)
            {
                return false;
            }

            if (roofDef != null)
            {
                RoofDef existingRoof = map.roofGrid.RoofAt(cell);
                if (existingRoof != null && existingRoof != roofDef)
                {
                    return false;
                }
            }

            List<Thing> things = map.thingGrid.ThingsListAtFast(cell);
            for (int i = 0; i < things.Count; i++)
            {
                if (BlocksPlacement(things[i].def))
                {
                    return false;
                }
            }

            // Vanish wipes whatever remains (wild plants, filth), which is intended.
            Thing rock = GenSpawn.Spawn(rockDef, cell, map, WipeMode.Vanish);
            if (rock == null || !rock.Spawned)
            {
                return false;
            }

            TerrainDef naturalTerrain = rockDef.building.naturalTerrain;
            if (terrain != naturalTerrain)
            {
                map.terrainGrid.SetTerrain(cell, naturalTerrain);
            }

            if (roofDef != null)
            {
                map.roofGrid.SetRoof(cell, roofDef);
            }

            return true;
        }

        private static bool BlocksPlacement(ThingDef def)
        {
            return def.category == ThingCategory.Pawn
                || def.category == ThingCategory.Item
                || def.category == ThingCategory.Building
                || def.IsBlueprint
                || def.IsFrame;
        }
    }
}
