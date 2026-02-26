using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralAddGasTypeContextKey = "BaseCheats.GeneralAddGas.SelectedGasType";

        private static void RegisterAddGas()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralAddGas",
                "CheatMenu.Cheat.GeneralAddGas.Label",
                "CheatMenu.Cheat.GeneralAddGas.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenGasSelectionWindow)
                    .AddTool(
                        AddGasAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenGasSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (GasType value in Enum.GetValues(typeof(GasType)))
            {
                GasType gasType = value;
                if ((gasType != GasType.ToxGas || ModsConfig.BiotechActive) && (gasType != GasType.DeadlifeDust || ModsConfig.AnomalyActive))
                {
                    options.Add(new FloatMenuOption(gasType.GetLabel().CapitalizeFirst(), delegate
                    {
                        context.Set(GeneralAddGasTypeContextKey, gasType);
                        continueFlow?.Invoke();
                    }));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void AddGasAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            GasType gasType;
            if (!context.TryGet(GeneralAddGasTypeContextKey, out gasType))
            {
                CheatMessageService.Message("CheatMenu.GeneralAddGas.Message.NoGasSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            GasUtility.AddGas(target.Cell, Find.CurrentMap, gasType, 5f);
        }
    }
}
