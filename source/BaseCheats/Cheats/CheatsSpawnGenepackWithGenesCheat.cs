using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class CheatsCategoryCheats
    {
        private const string SelectedGenepackGenesContextKey = "BaseCheats.Cheats.SpawnGenepackWithGenes.SelectedGenes";

        private static void RegisterSpawnGenepackWithGenes()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.Cheats.SpawnGenepackWithGenes",
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Label",
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Cheats")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireBiotech()
                    .AddWindow(OpenSpawnGenepackWithGenesWindow)
                    .AddTool(
                        SpawnSelectedGenepackAtCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenSpawnGenepackWithGenesWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new CheatsSpawnGenepackWithGenesWindow(delegate (List<GeneDef> selectedGenes)
            {
                context.Set(SelectedGenepackGenesContextKey, new List<GeneDef>(selectedGenes));
                continueFlow?.Invoke();
            }));
        }

        private static void SpawnSelectedGenepackAtCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedGenepackGenesContextKey, out List<GeneDef> selectedGenes) || selectedGenes.NullOrEmpty())
            {
                CheatMessageService.Message("CheatMenu.Cheats.SpawnGenepackWithGenes.Message.NoGenesSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 targetCell = target.Cell;
            Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
            genepack.Initialize(selectedGenes);
            GenSpawn.Spawn(genepack, targetCell, map);
            DebugActionsUtility.DustPuffFrom(genepack);

            CheatMessageService.Message(
                "CheatMenu.Cheats.SpawnGenepackWithGenes.Message.Result".Translate(genepack.LabelCap, selectedGenes.Count),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
