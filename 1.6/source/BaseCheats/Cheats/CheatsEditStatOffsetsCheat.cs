using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class CheatsCategoryCheats
    {
        private static void RegisterEditStatOffsets()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.Cheats.EditStatOffsets",
                "CheatMenu.Cheats.EditStatOffsets.Label",
                "CheatMenu.Cheats.EditStatOffsets.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Cheats")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenStatOffsetEditorAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat"));
        }

        private static void OpenStatOffsetEditorAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<ThingWithComps> thingsWithOffsetsComp = new List<ThingWithComps>();
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                ThingWithComps thingWithComps = thingsAtCell[i] as ThingWithComps;
                if (thingWithComps == null || !thingWithComps.TryGetComp(out CompCheatStatOffsets _))
                {
                    continue;
                }

                thingsWithOffsetsComp.Add(thingWithComps);
            }

            if (thingsWithOffsetsComp.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Cheats.EditStatOffsets.Message.NoCompOnCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (thingsWithOffsetsComp.Count == 1)
            {
                OpenEditorForThing(thingsWithOffsetsComp[0]);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            for (int i = 0; i < thingsWithOffsetsComp.Count; i++)
            {
                ThingWithComps optionThing = thingsWithOffsetsComp[i];
                options.Add(new FloatMenuOption(optionThing.LabelCap, delegate
                {
                    OpenEditorForThing(optionThing);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void OpenEditorForThing(ThingWithComps thing)
        {
            CompCheatStatOffsets comp = thing.GetComp<CompCheatStatOffsets>();
            Find.WindowStack.Add(new CheatsStatOffsetsEditorWindow(thing, comp));
        }
    }
}
