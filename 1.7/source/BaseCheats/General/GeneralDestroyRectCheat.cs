using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterDestroyRect()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralDestroyRect",
                "CheatMenu.General.DestroyRect.Label",
                "CheatMenu.General.DestroyRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(StartDestroyRectTool));
        }

        private static void StartDestroyRectTool(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();
            DebugToolsGeneral.GenericRectTool("Destroy", delegate (CellRect rect)
            {
                Map map = Find.CurrentMap;
                CellRect clippedRect = rect.ClipInsideMap(map);

                int affectedCellCount = 0;
                HashSet<Thing> thingsToDestroy = new HashSet<Thing>();
                foreach (IntVec3 cell in clippedRect)
                {
                    affectedCellCount++;
                    foreach (Thing thing in map.thingGrid.ThingsAt(cell).ToList())
                    {
                        thingsToDestroy.Add(thing);
                    }
                }

                int destroyedCount = 0;
                Thing.allowDestroyNonDestroyable = true;
                try
                {
                    foreach (Thing thing in thingsToDestroy)
                    {
                        thing.Destroy();
                        destroyedCount++;
                    }
                }
                finally
                {
                    Thing.allowDestroyNonDestroyable = false;
                }

                CheatMessageService.Message(
                    "CheatMenu.General.DestroyRect.Message.Result".Translate(destroyedCount, affectedCellCount),
                    destroyedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }
    }
}
