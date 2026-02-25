using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetBodyTypeCheat
    {
        private const string SelectedBodyTypeContextKey = "BaseCheats.Pawns.SetBodyType.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetBodyType",
                "CheatMenu.Cheat.PawnSetBodyType.Label",
                "CheatMenu.Cheat.PawnSetBodyType.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenBodyTypeSelectionWindow)
                    .AddTool(
                        ApplyBodyTypeToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnSetBodyType.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenBodyTypeSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnBodyTypeSelectionWindow(delegate (BodyTypeDef selected)
            {
                context.Set(SelectedBodyTypeContextKey, selected);
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

        private static void ApplyBodyTypeToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedBodyTypeContextKey, out BodyTypeDef selected))
            {
                CheatMessageService.Message("CheatMenu.PawnSetBodyType.Message.NoBodyTypeSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnSetBodyType.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.story == null)
            {
                CheatMessageService.Message("CheatMenu.PawnSetBodyType.Message.NoStory".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.story.bodyType = selected;
            pawn.Drawer.renderer.SetAllGraphicsDirty();

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnSetBodyType.Message.Result".Translate(pawn.LabelShortCap, selected.defName),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
