using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const int GeneralReplaceAllTradeShipsArrivalCount = 5;

        private static void RegisterReplaceAllTradeShips()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralReplaceAllTradeShips",
                "CheatMenu.General.ReplaceAllTradeShips.Label",
                "CheatMenu.General.ReplaceAllTradeShips.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(ReplaceAllTradeShips));
        }

        private static void ReplaceAllTradeShips(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            map.passingShipManager.DebugSendAllShipsAway();

            int successCount = 0;
            for (int i = 0; i < GeneralReplaceAllTradeShipsArrivalCount; i++)
            {
                IncidentParms incidentParms = new IncidentParms
                {
                    target = map
                };

                if (IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(incidentParms))
                {
                    successCount++;
                }
            }

            CheatMessageService.Message(
                "CheatMenu.General.ReplaceAllTradeShips.Message.Result".Translate(GeneralReplaceAllTradeShipsArrivalCount, successCount),
                successCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput,
                false);
        }
    }
}
