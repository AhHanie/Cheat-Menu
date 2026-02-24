using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetBackstoryCheat
    {
        private const string SelectedSlotContextKey = "BaseCheats.Pawns.SetBackstory.SelectedSlot";
        private const string SelectedBackstoryContextKey = "BaseCheats.Pawns.SetBackstory.SelectedBackstory";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetBackstory",
                "CheatMenu.Cheat.PawnSetBackstory.Label",
                "CheatMenu.Cheat.PawnSetBackstory.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenBackstorySlotSelectionWindow)
                    .AddWindow(OpenBackstorySelectionWindow)
                    .AddTool(
                        ApplyBackstoryToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetBackstory.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenBackstorySlotSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnBackstorySlotSelectionWindow(delegate (BackstorySlot selectedSlot)
            {
                context.Set(SelectedSlotContextKey, selectedSlot);
                continueFlow?.Invoke();
            }));
        }

        private static void OpenBackstorySelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            BackstorySlot selectedSlot;
            if (!context.TryGet(SelectedSlotContextKey, out selectedSlot))
            {
                CheatMessageService.Message("CheatMenu.PawnSetBackstory.Message.NoSlotSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new PawnBackstorySelectionWindow(selectedSlot, delegate (BackstorySelectionOption selectedBackstory)
            {
                context.Set(SelectedBackstoryContextKey, selectedBackstory);
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

        private static void ApplyBackstoryToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            BackstorySlot selectedSlot;
            if (!context.TryGet(SelectedSlotContextKey, out selectedSlot))
            {
                CheatMessageService.Message("CheatMenu.PawnSetBackstory.Message.NoSlotSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            BackstorySelectionOption selectedBackstory;
            if (!context.TryGet(SelectedBackstoryContextKey, out selectedBackstory) || selectedBackstory?.BackstoryDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetBackstory.Message.NoBackstorySelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetBackstory.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.story == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetBackstory.Message.NoStory".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                if (selectedSlot == BackstorySlot.Adulthood)
                {
                    pawn.story.Adulthood = selectedBackstory.BackstoryDef;
                }
                else
                {
                    pawn.story.Childhood = selectedBackstory.BackstoryDef;
                }

                MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
                DebugActionsUtility.DustPuffFrom(pawn);

                CheatMessageService.Message(
                    "CheatMenu.PawnSetBackstory.Message.Result".Translate(
                        pawn.LabelShortCap,
                        GetSlotLabel(selectedSlot),
                        selectedBackstory.DisplayLabel),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to set backstory '" + selectedBackstory.BackstoryDef.defName + "' for pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnSetBackstory.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }

        public static string GetSlotLabel(BackstorySlot slot)
        {
            return slot == BackstorySlot.Adulthood
                ? "CheatMenu.PawnSetBackstory.Slot.Adulthood".Translate()
                : "CheatMenu.PawnSetBackstory.Slot.Childhood".Translate();
        }
    }
}
