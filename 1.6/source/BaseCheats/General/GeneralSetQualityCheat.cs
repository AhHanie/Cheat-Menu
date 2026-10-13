using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralSetQualityContextKey = "BaseCheats.GeneralSetQuality.SelectedQuality";

        private static void RegisterSetQuality()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralSetQuality",
                "CheatMenu.General.SetQuality.Label",
                "CheatMenu.General.SetQuality.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenQualitySelectionWindow)
                    .AddTool(
                        SetQualityAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenQualitySelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (QualityCategory value in Enum.GetValues(typeof(QualityCategory)))
            {
                QualityCategory qualityInner = value;
                options.Add(new FloatMenuOption(qualityInner.ToString(), delegate
                {
                    context.Set(GeneralSetQualityContextKey, qualityInner);
                    continueFlow?.Invoke();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void SetQualityAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            QualityCategory quality;
            if (!context.TryGet(GeneralSetQualityContextKey, out quality))
            {
                CheatMessageService.Message("CheatMenu.General.SetQuality.Message.NoQualitySelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int updatedCount = 0;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                CompQuality qualityComp = thingsAtCell[i].TryGetComp<CompQuality>();
                if (qualityComp == null)
                {
                    continue;
                }

                qualityComp.SetQuality(quality, ArtGenerationContext.Outsider);
                updatedCount++;
            }

            CheatMessageService.Message(
                "CheatMenu.General.SetQuality.Message.Result".Translate(updatedCount, quality.ToString()),
                updatedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}

