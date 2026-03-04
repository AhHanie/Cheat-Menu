using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class InstantGravshipCheat
    {
        private const int DeckRadius = 6;

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.InstantGravship",
                "CheatMenu.Cheat.InstantGravship.Label",
                "CheatMenu.Cheat.InstantGravship.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Spawning")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddTool(
                        SpawnInstantGravshipAtCell,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void SpawnInstantGravshipAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            TerrainDef substructureDef = CheatMenuThingDefOf.Substructure;
            ThingDef gravEngineDef = CheatMenuThingDefOf.GravEngine;
            ThingDef pilotConsoleDef = CheatMenuThingDefOf.PilotConsole;
            ThingDef chemfuelTankDef = CheatMenuThingDefOf.ChemfuelTank;
            ThingDef smallThrusterDef = CheatMenuThingDefOf.SmallThruster;
            ThingDef gravshipHullDef = CheatMenuThingDefOf.GravshipHull;

            Map map = Find.CurrentMap;
            IntVec3 center = target.Cell;

            int substructureCount = PaintSubstructure(map, center, substructureDef);
            int hullCount = SpawnHullRing(map, center, gravshipHullDef);

            int componentCount = 0;
            componentCount += SpawnBuilding(map, gravEngineDef, center, Rot4.North) ? 1 : 0;
            componentCount += SpawnBuilding(map, pilotConsoleDef, center + new IntVec3(0, 0, -3), Rot4.North) ? 1 : 0;
            componentCount += SpawnBuilding(map, chemfuelTankDef, center + new IntVec3(-3, 0, 1), Rot4.North) ? 1 : 0;
            componentCount += SpawnBuilding(map, chemfuelTankDef, center + new IntVec3(3, 0, 1), Rot4.North) ? 1 : 0;
            componentCount += SpawnBuilding(map, smallThrusterDef, center + new IntVec3(0, 0, 6), Rot4.South) ? 1 : 0;
            componentCount += SpawnBuilding(map, smallThrusterDef, center + new IntVec3(0, 0, -6), Rot4.North) ? 1 : 0;
            componentCount += SpawnBuilding(map, smallThrusterDef, center + new IntVec3(-6, 0, 0), Rot4.East) ? 1 : 0;
            componentCount += SpawnBuilding(map, smallThrusterDef, center + new IntVec3(6, 0, 0), Rot4.West) ? 1 : 0;

            CheatMessageService.Message(
                "CheatMenu.InstantGravship.Message.Result".Translate(substructureCount, hullCount, componentCount),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static int PaintSubstructure(Map map, IntVec3 center, TerrainDef substructureDef)
        {
            int count = 0;
            CellRect deckRect = CellRect.CenteredOn(center, DeckRadius);
            foreach (IntVec3 cell in deckRect)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                map.terrainGrid.SetTerrain(cell, substructureDef);
                count++;
            }

            return count;
        }

        private static int SpawnHullRing(Map map, IntVec3 center, ThingDef gravshipHullDef)
        {
            int count = 0;
            CellRect hullRect = CellRect.CenteredOn(center, DeckRadius);
            foreach (IntVec3 cell in hullRect)
            {
                bool isEdgeCell = cell.x == hullRect.minX
                                  || cell.x == hullRect.maxX
                                  || cell.z == hullRect.minZ
                                  || cell.z == hullRect.maxZ;
                if (!isEdgeCell)
                {
                    continue;
                }

                bool isThrusterBackstopCell =
                    (cell.x == center.x && (cell.z == hullRect.minZ || cell.z == hullRect.maxZ))
                    || (cell.z == center.z && (cell.x == hullRect.minX || cell.x == hullRect.maxX));
                if (isThrusterBackstopCell)
                {
                    continue;
                }

                if (SpawnBuilding(map, gravshipHullDef, cell, Rot4.North))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool SpawnBuilding(Map map, ThingDef def, IntVec3 cell, Rot4 rotation)
        {
            if (!cell.InBounds(map))
            {
                return false;
            }

            Thing thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            Thing placedThing = GenSpawn.Spawn(thing, cell, map, rotation, WipeMode.Vanish);
            if (placedThing.def.CanHaveFaction)
            {
                placedThing.SetFaction(Faction.OfPlayer);
            }

            return true;
        }
    }
}





