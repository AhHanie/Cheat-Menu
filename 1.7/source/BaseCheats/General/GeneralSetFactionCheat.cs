using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterSetFaction()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralSetFaction",
                "CheatMenu.General.SetFaction.Label",
                "CheatMenu.General.SetFaction.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenSetFactionMenuAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenSetFactionMenuAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (!thingsAtCell.Any(x => x.def.CanHaveFaction))
            {
                CheatMessageService.Message("CheatMenu.General.SetFaction.Message.NoFactionableThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Faction faction in Find.FactionManager.AllFactionsInViewOrder)
            {
                Faction localFaction = faction;
                options.Add(new FloatMenuOption(localFaction.Name, delegate
                {
                    SetFactionForThings(thingsAtCell, localFaction);
                }));
            }

            options.Add(new FloatMenuOption("CheatMenu.General.SetFaction.Option.None".Translate(), delegate
            {
                SetFactionForThings(thingsAtCell, null);
            }));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void SetFactionForThings(List<Thing> things, Faction faction)
        {
            int updatedCount = 0;
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (!thing.def.CanHaveFaction)
                {
                    continue;
                }

                thing.SetFaction(faction);
                updatedCount++;
            }

            string factionLabel = faction != null ? faction.Name : "CheatMenu.General.SetFaction.Option.None".Translate().ToString();
            CheatMessageService.Message(
                "CheatMenu.General.SetFaction.Message.Result".Translate(updatedCount, factionLabel),
                updatedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}

