using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class MapFogCheats
    {
        public static void Register()
        {
            RegisterRefogMap();
            RegisterClearAllFog();
        }

        private static void RegisterRefogMap()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapRefogMap",
                "CheatMenu.Cheat.MapRefogMap.Label",
                "CheatMenu.Cheat.MapRefogMap.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(RefogMap));
        }

        private static void RegisterClearAllFog()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapClearAllFog",
                "CheatMenu.Cheat.MapClearAllFog.Label",
                "CheatMenu.Cheat.MapClearAllFog.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(ClearAllFog));
        }

        private static void RefogMap(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            FloodFillerFog.DebugRefogMap(map);
            CheatMessageService.Message(
                "CheatMenu.MapRefogMap.Message.Result".Translate(),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void ClearAllFog(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            map.fogGrid.ClearAllFog();
            CheatMessageService.Message(
                "CheatMenu.MapClearAllFog.Message.Result".Translate(),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
