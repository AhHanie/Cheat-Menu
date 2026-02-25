using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetHeadTypeCheat
    {
        private const string SelectedHeadTypeContextKey = "BaseCheats.Pawns.SetHeadType.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetHeadType",
                "CheatMenu.Cheat.PawnSetHeadType.Label",
                "CheatMenu.Cheat.PawnSetHeadType.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenHeadTypeSelectionWindow)
                    .AddTool(
                        ApplyHeadTypeToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetHeadType.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenHeadTypeSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnHeadTypeSelectionWindow(delegate (HeadTypeDef selected)
            {
                context.Set(SelectedHeadTypeContextKey, selected);
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

        private static void ApplyHeadTypeToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedHeadTypeContextKey, out HeadTypeDef selected))
            {
                CheatMessageService.Message("CheatMenu.PawnSetHeadType.Message.NoHeadTypeSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetHeadType.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.story == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetHeadType.Message.NoStory".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.story.headType = selected;
            pawn.Drawer.renderer.SetAllGraphicsDirty();

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnSetHeadType.Message.Result".Translate(pawn.LabelShortCap, selected.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
