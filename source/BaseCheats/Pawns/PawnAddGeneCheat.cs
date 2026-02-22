using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnAddGeneCheat
    {
        private const string SelectedGeneContextKey = "BaseCheats.Pawns.AddGene.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddGene",
                "CheatMenu.Cheat.PawnAddGene.Label",
                "CheatMenu.Cheat.PawnAddGene.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireBiotech()
                    .AddWindow(OpenGeneSelectionWindow)
                    .AddTool(
                        ApplyGeneToTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnAddGene.Message.SelectPawn"));
        }

        private static void OpenGeneSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnGeneSelectionWindow(delegate (GeneSelectionOption selected)
            {
                context.Set(SelectedGeneContextKey, selected);
                continueFlow?.Invoke();
            }));
        }

        private static TargetingParameters CreatePawnOrCellTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }

        private static void ApplyGeneToTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            GeneSelectionOption selected;
            if (!context.TryGet(SelectedGeneContextKey, out selected) || selected == null || selected.GeneDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.NoGeneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.InvalidTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> pawns = GetPawnsFromTarget(target, map);
            if (pawns.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.NoPawn".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int affected = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn?.genes == null)
                {
                    continue;
                }

                try
                {
                    pawn.genes.AddGene(selected.GeneDef, selected.IsXenogene);
                    affected++;
                }
                catch (Exception ex)
                {
                    UserLogger.Exception(ex, "Failed to add gene '" + selected.GeneDef.defName + "' to pawn '" + pawn?.LabelShortCap + "'");
                }
            }

            if (affected == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnAddGene.Message.NoGeneTracker".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnAddGene.Message.Result".Translate(selected.DisplayLabel, affected),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static List<Pawn> GetPawnsFromTarget(LocalTargetInfo target, Map map)
        {
            HashSet<Pawn> uniquePawns = new HashSet<Pawn>();

            if (target.HasThing && target.Thing is IThingHolder holder)
            {
                foreach (Pawn pawn in PawnsInside(holder))
                {
                    if (pawn != null && !pawn.Dead)
                    {
                        uniquePawns.Add(pawn);
                    }
                }
            }

            IntVec3 cell = target.Cell;
            if (cell.IsValid && cell.InBounds(map))
            {
                foreach (Pawn pawn in PawnsAt(cell, map))
                {
                    if (pawn != null && !pawn.Dead)
                    {
                        uniquePawns.Add(pawn);
                    }
                }
            }

            return uniquePawns.ToList();
        }

        private static IEnumerable<Pawn> PawnsAt(IntVec3 cell, Map map)
        {
            foreach (Thing thing in map.thingGrid.ThingsAt(cell))
            {
                if (thing is IThingHolder holder)
                {
                    foreach (Pawn pawn in PawnsInside(holder))
                    {
                        yield return pawn;
                    }
                }
            }
        }

        private static IEnumerable<Pawn> PawnsInside(IThingHolder holder)
        {
            if (holder is Pawn pawn)
            {
                yield return pawn;
            }

            ThingOwner directlyHeldThings = holder.GetDirectlyHeldThings();
            if (directlyHeldThings == null)
            {
                yield break;
            }

            for (int i = 0; i < directlyHeldThings.Count; i++)
            {
                if (directlyHeldThings[i] is IThingHolder childHolder)
                {
                    foreach (Pawn innerPawn in PawnsInside(childHolder))
                    {
                        yield return innerPawn;
                    }
                }
            }
        }
    }
}
