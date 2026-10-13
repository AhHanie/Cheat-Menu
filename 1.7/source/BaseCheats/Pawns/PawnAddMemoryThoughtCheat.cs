using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnAddMemoryThoughtCheat
    {
        private const string SelectedMemoryThoughtContextKey = "BaseCheats.Pawns.AddMemoryThought.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddMemoryThought",
                "CheatMenu.Cheat.PawnAddMemoryThought.Label",
                "CheatMenu.Cheat.PawnAddMemoryThought.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenMemoryThoughtSelectionWindow)
                    .AddTool(
                        ApplyMemoryThoughtToTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnAddMemoryThought.Message.SelectPawn"));
        }

        private static void OpenMemoryThoughtSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnMemoryThoughtSelectionWindow(delegate (ThoughtDef selected)
            {
                context.Set(SelectedMemoryThoughtContextKey, selected);
                continueFlow?.Invoke();
            }));
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

        private static void ApplyMemoryThoughtToTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedMemoryThoughtContextKey, out ThoughtDef selected))
            {
                CheatMessageService.Message("CheatMenu.PawnAddMemoryThought.Message.NoThoughtSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnAddMemoryThought.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            MemoryThoughtHandler memories = pawn.needs?.mood?.thoughts?.memories;
            if (memories == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnAddMemoryThought.Message.NoMoodMemoryTracker".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            memories.TryGainMemory(selected);
            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnAddMemoryThought.Message.Result".Translate(pawn.LabelShortCap, selected.defName),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
