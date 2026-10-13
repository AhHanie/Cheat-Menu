using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class SpawnThingCheat
    {
        private const string SpawnThingDefContextKey = "BaseCheats.SpawnThing.SelectedThingDef";
        private const string SpawnStackCountContextKey = "BaseCheats.SpawnThing.StackCount";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.SpawnThing",
                "CheatMenu.Cheat.SpawnThing.Label",
                "CheatMenu.Cheat.SpawnThing.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Spawning")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenSelectionWindow)
                    .AddTool(
                        SpawnSelectedThingAtCell,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat", repeatTargeting: true));
        }

        private static void OpenSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new SpawnThingSelectionWindow(delegate (ThingDef selectedThingDef)
            {
                context.Set(SpawnThingDefContextKey, selectedThingDef);
                context.Set(SpawnStackCountContextKey, 1);

                if (selectedThingDef.stackLimit > 1)
                {
                    Find.WindowStack.Add(new SpawnThingStackCountWindow(selectedThingDef, 1, delegate (int stackCount)
                    {
                        context.Set(SpawnStackCountContextKey, stackCount);
                        continueFlow?.Invoke();
                    }));
                    return;
                }

                continueFlow?.Invoke();
            }));
        }

        private static void SpawnSelectedThingAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            ThingDef selectedThingDef;
            if (!context.TryGet(SpawnThingDefContextKey, out selectedThingDef))
            {
                CheatMessageService.Message("CheatMenu.SpawnThing.Message.NoThingSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            IntVec3 targetCell = target.Cell;
            int desiredStackCount = context.Get(SpawnStackCountContextKey, 1);
            int maxStackCount = Mathf.Max(1, selectedThingDef.stackLimit);
            int stackCount = Mathf.Clamp(desiredStackCount, 1, maxStackCount);

            // Use RimWorld's own debug spawning path so stuff/quality/faction/etc. match vanilla behavior.
            DebugThingPlaceHelper.DebugSpawn(
                selectedThingDef,
                targetCell,
                stackCount,
                direct: false,
                thingStyleDef: null,
                canBeMinified: false,
                wipeMode: null);

            CheatMessageService.Message(
                "CheatMenu.SpawnThing.Message.Spawned".Translate(selectedThingDef.LabelCap, stackCount),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
