using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralTeleportPawnContextKey = "BaseCheats.GeneralTeleport.SelectedPawn";

        private static void RegisterTeleport()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralTeleport",
                "CheatMenu.General.Teleport.Label",
                "CheatMenu.General.Teleport.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        SelectPawnForTeleport,
                        CreatePawnTargetingParameters,
                        "CheatMenu.General.Teleport.Message.SelectPawn")
                    .AddTool(
                        TeleportSelectedPawnToCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.General.Teleport.Message.SelectDestination"));
        }

        private static void SelectPawnForTeleport(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead || !pawn.Spawned)
            {
                CheatMessageService.Message("CheatMenu.General.Teleport.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            context.Set(GeneralTeleportPawnContextKey, pawn);
        }

        private static void TeleportSelectedPawnToCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(GeneralTeleportPawnContextKey, out Pawn pawn))
            {
                CheatMessageService.Message("CheatMenu.General.Teleport.Message.NoPawnSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 destinationCell = target.Cell;
            if (!destinationCell.IsValid)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            pawn.Position = destinationCell.ClampInsideMap(map);
            pawn.Notify_Teleported();

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.General.Teleport.Message.Result".Translate(pawn.LabelShortCap, pawn.Position),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
