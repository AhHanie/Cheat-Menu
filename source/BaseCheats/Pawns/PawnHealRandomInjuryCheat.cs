using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnHealRandomInjuryCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnHealRandomInjury10",
                "CheatMenu.Cheat.PawnHealRandomInjury10.Label",
                "CheatMenu.Cheat.PawnHealRandomInjury10.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        HealRandomInjuryAtTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnHealRandomInjury10.Message.SelectTarget",
                        repeatTargeting: true));
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

        private static void HealRandomInjuryAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                CheatMessageService.Message("CheatMenu.PawnHealRandomInjury10.Message.InvalidTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> pawns = GetPawnsFromTarget(target, map);
            if (pawns.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnHealRandomInjury10.Message.NoPawn".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int healedCount = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (TryHealRandomInjury(pawns[i], 10f))
                {
                    healedCount++;
                }
            }

            if (healedCount == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnHealRandomInjury10.Message.NoHealable".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnHealRandomInjury10.Message.Result".Translate(healedCount, pawns.Count),
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

        private static bool TryHealRandomInjury(Pawn pawn, float amount)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return false;
            }

            List<Hediff_Injury> healableInjuries = new List<Hediff_Injury>();
            pawn.health.hediffSet.GetHediffs(
                ref healableInjuries,
                injury => injury.CanHealNaturally() || injury.CanHealFromTending());

            if (!healableInjuries.TryRandomElement(out Hediff_Injury selectedInjury))
            {
                return false;
            }

            selectedInjury.Heal(amount);
            return true;
        }
    }
}
