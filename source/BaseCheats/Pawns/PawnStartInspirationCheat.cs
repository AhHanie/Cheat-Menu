using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnStartInspirationCheat
    {
        private const string SelectedInspirationContextKey = "BaseCheats.Pawns.StartInspiration.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnStartInspiration",
                "CheatMenu.Cheat.PawnStartInspiration.Label",
                "CheatMenu.Cheat.PawnStartInspiration.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenInspirationSelectionWindow)
                    .AddTool(
                        ApplyInspirationToTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnStartInspiration.Message.SelectPawn"));
        }

        private static void OpenInspirationSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnInspirationSelectionWindow(delegate (InspirationSelectionOption selected)
            {
                context.Set(SelectedInspirationContextKey, selected);
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

        private static void ApplyInspirationToTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            InspirationSelectionOption selected;
            if (!context.TryGet(SelectedInspirationContextKey, out selected) || selected?.InspirationDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnStartInspiration.Message.NoInspirationSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnStartInspiration.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            bool started;
            try
            {
                started = pawn.mindState?.inspirationHandler != null
                    && pawn.mindState.inspirationHandler.TryStartInspiration(selected.InspirationDef, "Debug gain");
            }
            catch (Exception ex)
            {
                UserLogger.Exception(
                    ex,
                    "Failed to start inspiration '" + selected.InspirationDef.defName + "' on pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnStartInspiration.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if (!started)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnStartInspiration.Message.CouldNotStart".Translate(pawn.LabelShortCap, selected.DisplayLabel),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnStartInspiration.Message.Result".Translate(pawn.LabelShortCap, selected.DisplayLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
