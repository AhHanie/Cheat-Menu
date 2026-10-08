using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class MapNaturalRockCheats
    {
        private const string SelectedRockContextKey = "BaseCheats.Map.ExtendMountain.SelectedRock";
        private const string SelectedRoofModeContextKey = "BaseCheats.Map.ExtendMountain.SelectedRoofMode";
        private const string StartMapContextKey = "BaseCheats.Map.ExtendMountain.StartMap";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapExtendMountainCell",
                "CheatMenu.Cheat.MapExtendMountainCell.Label",
                "CheatMenu.Cheat.MapExtendMountainCell.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenRockSelectionWindow)
                    .AddWindow(OpenRoofSelectionWindow)
                    .AddAction(RememberCurrentMap)
                    .AddTool(
                        ExtendMountainAtCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.MapExtendMountain.Message.StartCellTool",
                        repeatTargeting: true));

            CheatRegistry.Register(
                "CheatMenu.Base.MapExtendMountainRect",
                "CheatMenu.Cheat.MapExtendMountainRect.Label",
                "CheatMenu.Cheat.MapExtendMountainRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenRockSelectionWindow)
                    .AddWindow(OpenRoofSelectionWindow)
                    .AddAction(StartRectTool));
        }

        private static void OpenRockSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new MapNaturalRockSelectionWindow(delegate (ThingDef selectedRock)
            {
                context.Set(SelectedRockContextKey, selectedRock);
                continueFlow?.Invoke();
            }));
        }

        private static void OpenRoofSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new MapNaturalRockRoofSelectionWindow(delegate (MapNaturalRockRoofMode selectedMode)
            {
                context.Set(SelectedRoofModeContextKey, selectedMode);
                continueFlow?.Invoke();
            }));
        }

        private static void RememberCurrentMap(CheatExecutionContext context)
        {
            context.Set(StartMapContextKey, Find.CurrentMap);
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

        private static void ExtendMountainAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map startMap = context.Get<Map>(StartMapContextKey);
            ThingDef rockDef = context.Get<ThingDef>(SelectedRockContextKey);
            MapNaturalRockRoofMode roofMode = context.Get(SelectedRoofModeContextKey, MapNaturalRockRoofMode.NoChange);

            PlaceAndReport(startMap, new[] { target.Cell }, rockDef, roofMode);
        }

        private static void StartRectTool(CheatExecutionContext context)
        {
            Map startMap = Find.CurrentMap;
            ThingDef rockDef = context.Get<ThingDef>(SelectedRockContextKey);
            MapNaturalRockRoofMode roofMode = context.Get(SelectedRoofModeContextKey, MapNaturalRockRoofMode.NoChange);

            Find.MainTabsRoot?.EscapeCurrentTab();
            CheatMessageService.Message(
                "CheatMenu.MapExtendMountain.Message.StartRectTool".Translate(rockDef.LabelCap),
                MessageTypeDefOf.NeutralEvent,
                false);

            DebugToolsGeneral.GenericRectTool(rockDef.LabelCap.ToString(), delegate (CellRect rect)
            {
                PlaceAndReport(startMap, rect, rockDef, roofMode);
            });
        }

        private static void PlaceAndReport(Map startMap, IEnumerable<IntVec3> cells, ThingDef rockDef, MapNaturalRockRoofMode roofMode)
        {
            if (Find.CurrentMap != startMap)
            {
                CheatMessageService.Message("CheatMenu.MapExtendMountain.Message.MapChanged".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            MapNaturalRockPlacementResult result = MapNaturalRockPlacement.Place(
                startMap,
                cells,
                rockDef,
                MapNaturalRockPlacement.GetRoofDef(roofMode));

            if (result.PlacedCount == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.MapExtendMountain.Message.NothingPlaced".Translate(result.SkippedCount),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.MapExtendMountain.Message.Result".Translate(rockDef.LabelCap, result.PlacedCount, result.SkippedCount),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
