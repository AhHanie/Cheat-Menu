using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnHealRandomInjuryCheat
    {
        private const string HealAmountContextKey = "BaseCheats.PawnHealRandomInjury.SelectedAmount";

        public static void Register()
        {
            RegisterHealRandomInjury10();
            RegisterHealRandomInjuryX();
        }

        private static void RegisterHealRandomInjury10()
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
                        HealRandomInjury10AtTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnHealRandomInjury10.Message.SelectTarget",
                        repeatTargeting: true));
        }

        private static void RegisterHealRandomInjuryX()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnHealRandomInjuryX",
                "CheatMenu.Cheat.PawnHealRandomInjuryX.Label",
                "CheatMenu.Cheat.PawnHealRandomInjuryX.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenHealAmountWindow)
                    .AddTool(
                        HealRandomInjuryXAtTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnHealRandomInjuryX.Message.SelectTarget",
                        repeatTargeting: true));
        }

        private static void OpenHealAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.PawnHealRandomInjuryX.Window.Title",
                "CheatMenu.PawnHealRandomInjuryX.Window.Description",
                initialAmount: 10,
                minAmount: 1,
                maxAmount: 1000,
                onConfirm: selectedAmount =>
                {
                    context.Set(HealAmountContextKey, selectedAmount);
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

        private static void HealRandomInjury10AtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            HealRandomInjuryAtTarget(
                target,
                amount: 10f,
                invalidTargetMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.InvalidTarget",
                noPawnMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.NoPawn",
                noHealableMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.NoHealable",
                resultMessageKey: "CheatMenu.PawnHealRandomInjury10.Message.Result",
                includeAmountInResultMessage: false);
        }

        private static void HealRandomInjuryXAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(HealAmountContextKey, 0);
            if (selectedAmount <= 0)
            {
                CheatMessageService.Message("CheatMenu.PawnHealRandomInjuryX.Message.NoAmountSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            HealRandomInjuryAtTarget(
                target,
                amount: selectedAmount,
                invalidTargetMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.InvalidTarget",
                noPawnMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.NoPawn",
                noHealableMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.NoHealable",
                resultMessageKey: "CheatMenu.PawnHealRandomInjuryX.Message.Result",
                includeAmountInResultMessage: true);
        }

        private static void HealRandomInjuryAtTarget(
            LocalTargetInfo target,
            float amount,
            string invalidTargetMessageKey,
            string noPawnMessageKey,
            string noHealableMessageKey,
            string resultMessageKey,
            bool includeAmountInResultMessage)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                CheatMessageService.Message(invalidTargetMessageKey.Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> pawns = GetPawnsFromTarget(target, map);
            if (pawns.Count == 0)
            {
                CheatMessageService.Message(noPawnMessageKey.Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int healedCount = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (TryHealRandomInjury(pawns[i], amount))
                {
                    healedCount++;
                }
            }

            if (healedCount == 0)
            {
                CheatMessageService.Message(noHealableMessageKey.Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            string resultMessage = includeAmountInResultMessage
                ? resultMessageKey.Translate(healedCount, pawns.Count, amount)
                : resultMessageKey.Translate(healedCount, pawns.Count);

            CheatMessageService.Message(
                resultMessage,
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
