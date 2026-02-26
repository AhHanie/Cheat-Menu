using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralChangeWeatherContextKey = "BaseCheats.GeneralChangeWeather.SelectedWeather";

        private static void RegisterChangeWeather()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralChangeWeather",
                "CheatMenu.Cheat.GeneralChangeWeather.Label",
                "CheatMenu.Cheat.GeneralChangeWeather.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenWeatherSelectionWindow)
                    .AddAction(ChangeToSelectedWeather));
        }

        private static void OpenWeatherSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (WeatherDef allDef in DefDatabase<WeatherDef>.AllDefs)
            {
                WeatherDef localWeather = allDef;
                options.Add(new FloatMenuOption(localWeather.LabelCap, delegate
                {
                    context.Set(GeneralChangeWeatherContextKey, localWeather);
                    continueFlow?.Invoke();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void ChangeToSelectedWeather(CheatExecutionContext context)
        {
            if (!context.TryGet(GeneralChangeWeatherContextKey, out WeatherDef selectedWeather))
            {
                CheatMessageService.Message("CheatMenu.GeneralChangeWeather.Message.NoWeatherSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.CurrentMap.weatherManager.TransitionTo(selectedWeather);
        }
    }
}
