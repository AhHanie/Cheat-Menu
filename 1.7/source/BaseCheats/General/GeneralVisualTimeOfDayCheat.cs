using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralVisualTimeOfDayContextKey = "BaseCheats.GeneralVisualTimeOfDay.SelectedPreset";

        private static readonly VisualTimeOfDay[] VisualTimeOfDaySelectableValues =
        {
            VisualTimeOfDay.Normal,
            VisualTimeOfDay.Dawn,
            VisualTimeOfDay.Noon,
            VisualTimeOfDay.Dusk,
            VisualTimeOfDay.Midnight
        };

        private static void RegisterVisualTimeOfDay()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralVisualTimeOfDay",
                "CheatMenu.General.VisualTimeOfDay.Label",
                "CheatMenu.General.VisualTimeOfDay.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenVisualTimeOfDaySelectionWindow)
                    .AddAction(ApplySelectedVisualTimeOfDay));
        }

        private static void OpenVisualTimeOfDaySelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            for (int i = 0; i < VisualTimeOfDaySelectableValues.Length; i++)
            {
                VisualTimeOfDay preset = VisualTimeOfDaySelectableValues[i];
                options.Add(new FloatMenuOption(GetVisualTimeOfDayLabelKey(preset).Translate(), delegate
                {
                    context.Set(GeneralVisualTimeOfDayContextKey, preset);
                    continueFlow?.Invoke();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void ApplySelectedVisualTimeOfDay(CheatExecutionContext context)
        {
            if (!context.TryGet(GeneralVisualTimeOfDayContextKey, out VisualTimeOfDay selectedPreset))
            {
                return;
            }

            CheatMenuMapComponent mapComponent = Find.CurrentMap.GetComponent<CheatMenuMapComponent>();
            mapComponent.VisualTimeOfDayPreset = selectedPreset;
            VisualTimeOfDayPatchController.Refresh();

            CheatMessageService.Message(
                "CheatMenu.General.VisualTimeOfDay.Message.Result".Translate(GetVisualTimeOfDayLabelKey(selectedPreset).Translate()),
                MessageTypeDefOf.NeutralEvent,
                false);
        }

        private static string GetVisualTimeOfDayLabelKey(VisualTimeOfDay preset)
        {
            switch (preset)
            {
                case VisualTimeOfDay.Dawn:
                    return "CheatMenu.General.VisualTimeOfDay.Option.Dawn";
                case VisualTimeOfDay.Noon:
                    return "CheatMenu.General.VisualTimeOfDay.Option.Noon";
                case VisualTimeOfDay.Dusk:
                    return "CheatMenu.General.VisualTimeOfDay.Option.Dusk";
                case VisualTimeOfDay.Midnight:
                    return "CheatMenu.General.VisualTimeOfDay.Option.Midnight";
                default:
                    return "CheatMenu.General.VisualTimeOfDay.Option.Normal";
            }
        }
    }
}
