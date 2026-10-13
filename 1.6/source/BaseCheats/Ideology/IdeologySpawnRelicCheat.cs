using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class IdeologyCheats
    {
        private const string IdeologySpawnRelicContextKey = "BaseCheats.IdeologySpawnRelic.SelectedRelicPrecept";

        private static void RegisterSpawnRelic()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.IdeologySpawnRelic",
                "CheatMenu.Ideology.SpawnRelic.Label",
                "CheatMenu.Ideology.SpawnRelic.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Ideology")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddWindow(OpenRelicSelectionWindow)
                    .AddTool(
                        SpawnSelectedRelicAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Ideology.SpawnRelic.Message.SelectCell",
                        repeatTargeting: true));
        }

        private static void OpenRelicSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            List<Precept_Relic> relicPrecepts = IdeologyRelicSelectionWindow.BuildRelicPreceptList();
            if (relicPrecepts.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Ideology.SpawnRelic.Message.NoRelicsAvailable".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new IdeologyRelicSelectionWindow(relicPrecepts, selectedRelic =>
            {
                context.Set(IdeologySpawnRelicContextKey, selectedRelic);
                continueFlow?.Invoke();
            }));
        }

        private static void SpawnSelectedRelicAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(IdeologySpawnRelicContextKey, out Precept_Relic selectedRelic))
            {
                CheatMessageService.Message("CheatMenu.Ideology.SpawnRelic.Message.NoRelicSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            Thing relicThing = selectedRelic.GenerateRelic();
            GenSpawn.Spawn(relicThing, target.Cell, map);

            CheatMessageService.Message(
                "CheatMenu.Ideology.SpawnRelic.Message.Result".Translate(relicThing.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
