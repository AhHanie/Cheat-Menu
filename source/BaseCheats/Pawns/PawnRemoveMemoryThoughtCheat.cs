using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnRemoveMemoryThoughtCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnRemoveMemoryThought",
                "CheatMenu.Cheat.PawnRemoveMemoryThought.Label",
                "CheatMenu.Cheat.PawnRemoveMemoryThought.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenRemoveMemoryThoughtWindowForPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnRemoveMemoryThought.Message.SelectPawn"));
        }

        private static TargetingParameters CreatePawnTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = false,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }

        private static void OpenRemoveMemoryThoughtWindowForPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnRemoveMemoryThought.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            MemoryThoughtHandler memories = pawn.needs?.mood?.thoughts?.memories;
            if (memories == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnRemoveMemoryThought.Message.NoMoodMemoryTracker".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            List<Thought_Memory> memoryList = memories.Memories.ToList();
            if (memoryList.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnRemoveMemoryThought.Message.NoMemories".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            Find.WindowStack.Add(new PawnRemoveMemoryThoughtSelectionWindow(pawn, memoryList, delegate (Thought_Memory selected)
            {
                string removedLabel = selected.LabelCap;
                memories.RemoveMemory(selected);
                DebugActionsUtility.DustPuffFrom(pawn);
                CheatMessageService.Message(
                    "CheatMenu.PawnRemoveMemoryThought.Message.Result".Translate(pawn.LabelShortCap, removedLabel),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }));
        }
    }
}
