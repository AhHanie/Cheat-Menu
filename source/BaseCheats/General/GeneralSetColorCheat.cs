using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralSetColorOptionContextKey = "BaseCheats.GeneralSetColor.SelectedOption";

        private static void RegisterSetColor()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralSetColor",
                "CheatMenu.General.SetColor.Label",
                "CheatMenu.General.SetColor.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenColorSelectionWindow)
                    .AddTool(
                        SetColorAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenColorSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new GeneralSetColorSelectionWindow(delegate (GeneralSetColorOption selectedOption)
            {
                context.Set(GeneralSetColorOptionContextKey, selectedOption);
                continueFlow?.Invoke();
            }));
        }

        private static void SetColorAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(GeneralSetColorOptionContextKey, out GeneralSetColorOption selectedOption))
            {
                CheatMessageService.Message("CheatMenu.General.SetColor.Message.NoColorSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = Find.CurrentMap.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Color color = selectedOption.ResolveColor();
            int updatedCount = 0;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                Pawn pawn = thingsAtCell[i] as Pawn;
                if (pawn?.apparel != null)
                {
                    for (int j = 0; j < pawn.apparel.WornApparel.Count; j++)
                    {
                        pawn.apparel.WornApparel[j].SetColor(color, reportFailure: false);
                        updatedCount++;
                    }

                    continue;
                }

                thingsAtCell[i].SetColor(color, reportFailure: false);
                updatedCount++;
            }

            CheatMessageService.Message(
                "CheatMenu.General.SetColor.Message.Result".Translate(updatedCount, selectedOption.Label),
                updatedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
