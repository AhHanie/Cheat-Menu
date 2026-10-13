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
            if (!context.TryGet(SpawnPawnKindContextKey, out PawnKindDef selectedPawnKind) || selectedPawnKind == null)
            {
                CheatMessageService.Message("CheatMenu.SpawnPawn.Message.NoPawnKindSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 targetCell = target.Cell;
            Faction faction = FactionUtility.DefaultFactionFrom(selectedPawnKind.defaultFactionDef);
            Pawn pawn = PawnGenerator.GeneratePawn(selectedPawnKind, faction, map.Tile);
            GenSpawn.Spawn(pawn, targetCell, map);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                "CheatMenu.SpawnPawn.Message.Spawned".Translate(pawn.LabelShortCap, selectedPawnKind.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
