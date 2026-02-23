using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class SpawnPawnCheat
    {
        private const string SpawnPawnKindContextKey = "BaseCheats.SpawnPawn.SelectedPawnKindDef";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.SpawnPawn",
                "CheatMenu.Cheat.SpawnPawn.Label",
                "CheatMenu.Cheat.SpawnPawn.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Spawning")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenSelectionWindow)
                    .AddTool(
                        SpawnSelectedPawnAtCell,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new SpawnPawnSelectionWindow(delegate (PawnKindDef selectedPawnKind)
            {
                context.Set(SpawnPawnKindContextKey, selectedPawnKind);
                continueFlow?.Invoke();
            }));
        }

        private static void SpawnSelectedPawnAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            PawnKindDef selectedPawnKind;
            if (!context.TryGet(SpawnPawnKindContextKey, out selectedPawnKind) || selectedPawnKind == null)
            {
                CheatMessageService.Message("CheatMenu.SpawnPawn.Message.NoPawnKindSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 targetCell = target.Cell;
            if (map == null || !targetCell.IsValid || !targetCell.InBounds(map))
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                Faction faction = FactionUtility.DefaultFactionFrom(selectedPawnKind.defaultFactionDef);
                Pawn pawn = PawnGenerator.GeneratePawn(selectedPawnKind, faction, map.Tile);
                if (pawn == null)
                {
                    throw new InvalidOperationException("Pawn generation returned null for PawnKindDef '" + selectedPawnKind.defName + "'.");
                }

                GenSpawn.Spawn(pawn, targetCell, map);
                DebugActionsUtility.DustPuffFrom(pawn);

                CheatMessageService.Message(
                    "CheatMenu.SpawnPawn.Message.Spawned".Translate(pawn.LabelShortCap, selectedPawnKind.LabelCap),
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to spawn pawn for PawnKindDef '" + selectedPawnKind.defName + "'.");
                CheatMessageService.Message(
                    "CheatMenu.SpawnPawn.Message.PlaceFailed".Translate(selectedPawnKind.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
            }
        }
    }
}
