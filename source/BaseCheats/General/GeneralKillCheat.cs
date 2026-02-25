using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterKill()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralKill",
                "CheatMenu.Cheat.GeneralKill.Label",
                "CheatMenu.Cheat.GeneralKill.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        KillAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void KillAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int killAttemptCount = 0;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                thingsAtCell[i].Kill();
                killAttemptCount++;
            }

            CheatMessageService.Message(
                "CheatMenu.GeneralKill.Message.Result".Translate(killAttemptCount),
                killAttemptCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
