using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnGiveAbilityCheat
    {
        private const string SelectedAbilityContextKey = "BaseCheats.Pawns.GiveAbility.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnGiveAbility",
                "CheatMenu.Cheat.PawnGiveAbility.Label",
                "CheatMenu.Cheat.PawnGiveAbility.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenAbilitySelectionWindow)
                    .AddTool(
                        ApplyAbilityToTarget,
                        CreatePawnOrCellTargetingParameters,
                        "CheatMenu.PawnGiveAbility.Message.SelectPawn"));
        }

        private static void OpenAbilitySelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnAbilitySelectionWindow(delegate (AbilitySelectionOption selected)
            {
                context.Set(SelectedAbilityContextKey, selected);
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

        private static void ApplyAbilityToTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            AbilitySelectionOption selected;
            if (!context.TryGet(SelectedAbilityContextKey, out selected) || selected == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoAbilitySelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.InvalidTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> pawns = GetPawnsFromTarget(target, map);
            if (pawns.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoPawn".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int affected = 0;
            if (selected.IsAll)
            {
                for (int i = 0; i < pawns.Count; i++)
                {
                    if (TryGrantAllAbilities(pawns[i]))
                    {
                        affected++;
                    }
                }

                if (affected == 0)
                {
                    CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoAbilityTracker".Translate(), MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                CheatMessageService.Message(
                    "CheatMenu.PawnGiveAbility.Message.ResultAll".Translate(affected),
                    MessageTypeDefOf.PositiveEvent,
                    false);
                return;
            }

            if (selected.AbilityDef == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoAbilitySelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                if (TryGrantAbility(pawns[i], selected.AbilityDef))
                {
                    affected++;
                }
            }

            if (affected == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoAbilityTracker".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnGiveAbility.Message.Result".Translate(selected.DisplayLabel, affected),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static bool TryGrantAbility(Pawn pawn, AbilityDef abilityDef)
        {
            if (pawn?.abilities == null || abilityDef == null)
            {
                return false;
            }

            try
            {
                pawn.abilities.GainAbility(abilityDef);
                return true;
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to add ability '" + abilityDef.defName + "' to pawn '" + pawn.LabelShortCap + "'");
                return false;
            }
        }

        private static bool TryGrantAllAbilities(Pawn pawn)
        {
            if (pawn?.abilities == null)
            {
                return false;
            }

            foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
            {
                if (abilityDef == null)
                {
                    continue;
                }

                try
                {
                    pawn.abilities.GainAbility(abilityDef);
                }
                catch (Exception ex)
                {
                    UserLogger.Exception(ex, "Failed to add ability '" + abilityDef.defName + "' to pawn '" + pawn.LabelShortCap + "'");
                }
            }

            return true;
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
