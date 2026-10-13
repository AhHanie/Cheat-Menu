using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class MapSetTerrainRectCheat
    {
        private const string SelectedTerrainContextKey = "BaseCheats.Map.SetTerrainRect.SelectedTerrain";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapSetTerrainRect",
                "CheatMenu.Cheat.MapSetTerrainRect.Label",
                "CheatMenu.Cheat.MapSetTerrainRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenTerrainSelectionWindow)
                    .AddAction(StartTerrainRectTool));
        }

        private static void OpenTerrainSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new MapTerrainSelectionWindow(delegate (TerrainDef selectedTerrain)
            {
                context.Set(SelectedTerrainContextKey, selectedTerrain);
                continueFlow?.Invoke();
            }));
        }

        private static void StartTerrainRectTool(CheatExecutionContext context)
        {
            TerrainDef selectedTerrain;
            if (!context.TryGet(SelectedTerrainContextKey, out selectedTerrain) || selectedTerrain == null)
            {
                CheatMessageService.Message("CheatMenu.MapSetTerrainRect.Message.NoTerrainSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.MainTabsRoot?.EscapeCurrentTab();
            CheatMessageService.Message(
                "CheatMenu.MapSetTerrainRect.Message.StartRectTool".Translate(selectedTerrain.defName),
                MessageTypeDefOf.NeutralEvent,
                false);

            DebugToolsGeneral.GenericRectTool(selectedTerrain.defName, delegate (CellRect rect)
            {
                Map map = Find.CurrentMap;
                int changedCount = 0;
                foreach (IntVec3 cell in rect)
                {
                    map.terrainGrid.SetTerrain(cell, selectedTerrain);
                    changedCount++;
                }

                CheatMessageService.Message(
                    "CheatMenu.MapSetTerrainRect.Message.Result".Translate(changedCount, selectedTerrain.defName),
                    changedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }
    }
}
