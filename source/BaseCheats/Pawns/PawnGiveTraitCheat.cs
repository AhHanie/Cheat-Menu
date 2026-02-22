using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnGiveTraitCheat
    {
        private const string SelectedTraitContextKey = "BaseCheats.Pawns.GiveTrait.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnGiveTrait",
                "CheatMenu.Cheat.PawnGiveTrait.Label",
                "CheatMenu.Cheat.PawnGiveTrait.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenTraitSelectionWindow)
                    .AddTool(
                        ApplyTraitToTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnGiveTrait.Message.SelectPawn"));
        }

        private static void OpenTraitSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnTraitSelectionWindow(delegate (TraitSelection selected)
            {
                context.Set(SelectedTraitContextKey, selected);
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

        private static void ApplyTraitToTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            TraitSelection selected;
            if (!context.TryGet(SelectedTraitContextKey, out selected) || selected == null || selected.TraitDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveTrait.Message.NoTraitSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveTrait.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.story?.traits == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveTrait.Message.NoStory".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                pawn.story.traits.GainTrait(new Trait(selected.TraitDef, selected.Degree), suppressConflicts: true);
                CheatMessageService.Message(
                    "CheatMenu.PawnGiveTrait.Message.Result".Translate(pawn.LabelShortCap, selected.DisplayLabel),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to add trait to pawn '" + pawn.LabelShortCap + "'");
                CheatMessageService.Message(
                    "CheatMenu.Message.ExecutionFailed".Translate("CheatMenu.Cheat.PawnGiveTrait.Label".Translate()),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
