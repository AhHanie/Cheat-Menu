using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralDamageAmountContextKey = "BaseCheats.GeneralDamage.SelectedAmount";

        private static void RegisterDamage10()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralDamage10",
                "CheatMenu.Cheat.GeneralDamage10.Label",
                "CheatMenu.Cheat.GeneralDamage10.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        Damage10AtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void RegisterDamageX()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralDamageX",
                "CheatMenu.Cheat.GeneralDamageX.Label",
                "CheatMenu.Cheat.GeneralDamageX.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenDamageAmountWindow)
                    .AddTool(
                        DamageXAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenDamageAmountWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.GeneralDamageX.Window.Title",
                "CheatMenu.GeneralDamageX.Window.Description",
                initialAmount: 10,
                minAmount: 1,
                maxAmount: 1000,
                onConfirm: selectedAmount =>
                {
                    context.Set(GeneralDamageAmountContextKey, selectedAmount);
                    continueFlow?.Invoke();
                }));
        }

        private static void Damage10AtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            DamageAtTargetCell(target, 10f, "CheatMenu.GeneralDamage10.Message.Result");
        }

        private static void DamageXAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            int selectedAmount = context.Get(GeneralDamageAmountContextKey, 0);
            if (selectedAmount <= 0)
            {
                CheatMessageService.Message("CheatMenu.GeneralDamageX.Message.NoAmountSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            DamageAtTargetCell(target, selectedAmount, "CheatMenu.GeneralDamageX.Message.Result");
        }

        private static void DamageAtTargetCell(LocalTargetInfo target, float amount, string resultMessageKey)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            if (map == null || !cell.IsValid || !cell.InBounds(map))
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int damageAttemptCount = 0;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                Thing thing = thingsAtCell[i];
                if (thing == null || thing.Destroyed)
                {
                    continue;
                }

                thing.TakeDamage(new DamageInfo(DamageDefOf.Crush, amount));
                damageAttemptCount++;
            }

            CheatMessageService.Message(
                resultMessageKey.Translate(damageAttemptCount, amount),
                damageAttemptCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
