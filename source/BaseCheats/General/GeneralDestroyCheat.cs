using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterDestroy()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralDestroy",
                "CheatMenu.Cheat.GeneralDestroy.Label",
                "CheatMenu.Cheat.GeneralDestroy.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        DestroyAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void DestroyAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int destroyedCount = 0;
            Thing.allowDestroyNonDestroyable = true;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                thingsAtCell[i].Destroy();
                destroyedCount++;
            }
            Thing.allowDestroyNonDestroyable = false;

            CheatMessageService.Message(
                "CheatMenu.GeneralDestroy.Message.Result".Translate(destroyedCount),
                destroyedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
