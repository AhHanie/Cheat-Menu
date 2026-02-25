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
            if (!context.TryGet(SelectedAbilityContextKey, out selected))
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoAbilitySelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null)
            {
                CheatMessageService.Message("CheatMenu.PawnGiveAbility.Message.NoPawn".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (selected.IsAll)
            {
                TryGrantAllAbilities(pawn);

                CheatMessageService.Message(
                    "CheatMenu.PawnGiveAbility.Message.ResultAll".Translate(),
                    MessageTypeDefOf.PositiveEvent,
                    false);
                return;
            }

            TryGrantAbility(pawn, selected.AbilityDef);

            CheatMessageService.Message(
                "CheatMenu.PawnGiveAbility.Message.Result".Translate(selected.DisplayLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static bool TryGrantAbility(Pawn pawn, AbilityDef abilityDef)
        {
            if (pawn.abilities == null)
            {
                return false;
            }

            pawn.abilities.GainAbility(abilityDef);
            return true;
        }

        private static bool TryGrantAllAbilities(Pawn pawn)
        {
            if (pawn.abilities == null)
            {
                return false;
            }

            foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
            {
                pawn.abilities.GainAbility(abilityDef);
            }

            return true;
        }
    }
}
