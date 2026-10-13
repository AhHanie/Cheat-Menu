using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class MapSeedFishPopulationCheat
    {
        private static readonly FieldInfo ShouldHaveFishField = AccessTools.Field(typeof(WaterBody), "shouldHaveFish");
        private static readonly FieldInfo CommonFishField = AccessTools.Field(typeof(WaterBody), "commonFish");

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapSeedFishPopulation",
                "CheatMenu.Cheat.MapSeedFishPopulation.Label",
                "CheatMenu.Cheat.MapSeedFishPopulation.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddTool(
                        SeedFishPopulationAtCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.MapSeedFishPopulation.Message.SelectCell",
                        repeatTargeting: true));
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

        private static void SeedFishPopulationAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            if (map == null || !cell.IsValid || !cell.InBounds(map))
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            TerrainDef terrain = cell.GetTerrain(map);
            if (terrain == null || !terrain.IsWater)
            {
                CheatMessageService.Message("CheatMenu.MapSeedFishPopulation.Message.NotWater".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            WaterBody body = map.waterBodyTracker?.WaterBodyAt(cell);
            if (body == null)
            {
                CheatMessageService.Message("CheatMenu.MapSeedFishPopulation.Message.NoWaterBody".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (body.waterBodyType != WaterBodyType.Freshwater && body.waterBodyType != WaterBodyType.Saltwater)
            {
                CheatMessageService.Message("CheatMenu.MapSeedFishPopulation.Message.UnsupportedWaterType".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (body.CommonFish.Any() || body.UncommonFish.Any())
            {
                body.Population = body.MaxPopulation;
                CheatMessageService.Message(
                    "CheatMenu.MapSeedFishPopulation.Message.Refilled".Translate(body.Population.ToString("F0")),
                    MessageTypeDefOf.PositiveEvent,
                    false);
                return;
            }

            if (!TrySeedFishType(map, body, out ThingDef seededFish))
            {
                CheatMessageService.Message("CheatMenu.MapSeedFishPopulation.Message.NoCompatibleFish".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            body.Population = body.MaxPopulation;
            CheatMessageService.Message(
                "CheatMenu.MapSeedFishPopulation.Message.Seeded".Translate(seededFish.LabelCap, body.Population.ToString("F0")),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static bool TrySeedFishType(Map map, WaterBody body, out ThingDef seededFish)
        {
            seededFish = null;
            if (ShouldHaveFishField == null || CommonFishField == null)
            {
                return false;
            }

            BiomeFishTypes fishTypes = map.Biome?.fishTypes;
            if (fishTypes == null)
            {
                return false;
            }

            List<FishChance> candidates = body.waterBodyType == WaterBodyType.Freshwater
                ? fishTypes.freshwater_Common
                : fishTypes.saltwater_Common;
            if (candidates.NullOrEmpty() || !candidates.TryRandomElement(out FishChance chosen) || chosen?.fishDef == null)
            {
                return false;
            }

            List<ThingDef> commonFish = (List<ThingDef>)CommonFishField.GetValue(body);
            commonFish.Add(chosen.fishDef);
            ShouldHaveFishField.SetValue(body, true);

            seededFish = chosen.fishDef;
            return true;
        }
    }
}
