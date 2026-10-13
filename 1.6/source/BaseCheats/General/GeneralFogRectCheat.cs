using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterFogRect()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralFogRect",
                "CheatMenu.General.FogRect.Label",
                "CheatMenu.General.FogRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(StartFogRectTool));
        }

        private static void RegisterUnfogRect()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralUnfogRect",
                "CheatMenu.General.UnfogRect.Label",
                "CheatMenu.General.UnfogRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(StartUnfogRectTool));
        }

        private static void StartFogRectTool(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            Find.MainTabsRoot?.EscapeCurrentTab();
            DebugToolsGeneral.GenericRectTool("Fog", delegate (CellRect rect)
            {
                Map currentMap = Find.CurrentMap;
                CellRect clippedRect = rect.ClipInsideMap(currentMap);
                int affectedCount = 0;
                foreach (IntVec3 _ in clippedRect)
                {
                    affectedCount++;
                }

                currentMap.fogGrid.Refog(clippedRect);
                CheatMessageService.Message(
                    "CheatMenu.General.FogRect.Message.Result".Translate(affectedCount),
                    affectedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }

        private static void StartUnfogRectTool(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            Find.MainTabsRoot?.EscapeCurrentTab();
            DebugToolsGeneral.GenericRectTool("Clear", delegate (CellRect rect)
            {
                Map currentMap = Find.CurrentMap;
                int affectedCount = 0;
                foreach (IntVec3 cell in rect.ClipInsideMap(currentMap))
                {
                    currentMap.fogGrid.Unfog(cell);
                    affectedCount++;
                }

                CheatMessageService.Message(
                    "CheatMenu.General.UnfogRect.Message.Result".Translate(affectedCount),
                    affectedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }
    }
}

