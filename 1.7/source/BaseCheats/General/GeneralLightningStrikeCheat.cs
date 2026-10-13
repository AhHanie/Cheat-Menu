using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralLightningStrikeDelayContextKey = "BaseCheats.GeneralLightningStrike.SelectedDelayTicks";

        private static void RegisterLightningStrike()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralLightningStrike",
                "CheatMenu.General.LightningStrike.Label",
                "CheatMenu.General.LightningStrike.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        LightningStrikeAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void RegisterLightningStrikeDelayed()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralLightningStrikeDelayed",
                "CheatMenu.General.LightningStrikeDelayed.Label",
                "CheatMenu.General.LightningStrikeDelayed.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenLightningStrikeDelayWindow)
                    .AddTool(
                        LightningStrikeDelayedAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenLightningStrikeDelayWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            for (int i = 0; i <= 40; i++)
            {
                int delayTicks = i * 30;
                float seconds = delayTicks / 60f;
                options.Add(new FloatMenuOption(
                    "CheatMenu.General.LightningStrikeDelayed.Option.Seconds".Translate(seconds.ToString("F1")),
                    delegate
                    {
                        context.Set(GeneralLightningStrikeDelayContextKey, delayTicks);
                        continueFlow?.Invoke();
                    }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void LightningStrikeAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, target.Cell));
        }

        private static void LightningStrikeDelayedAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            int delayTicks = context.Get(GeneralLightningStrikeDelayContextKey, 30);
            Map map = Find.CurrentMap;
            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeDelayed(map, target.Cell, delayTicks));
        }
    }
}

