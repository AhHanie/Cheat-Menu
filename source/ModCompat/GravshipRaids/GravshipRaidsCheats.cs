using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class GravshipRaidsCheats
    {
        private const string CategoryKey = "CheatMenu.Category.GravshipRaids";
        private const string SelectedTemplateContextKey = "ModCompat.GravshipRaids.SelectedTemplate";
        private static bool registered;

        public static void Register()
        {
            if (registered)
            {
                return;
            }

            registered = true;
            RegisterRotateTemplateSpawnCheat();
            RegisterSpawnTemplateCheat();
            RegisterTestLandingSearchCheat();
            RegisterForceGravshipRaidCheat();
            RegisterForceGravshipRaidWithTemplateCheat();
            RegisterForceEnemyGravshipDepartureCheat();
            RegisterCapturePrefabWithTerrainCheat();
        }

        private static void RegisterRotateTemplateSpawnCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.RotateTemplateSpawn",
                "CheatMenu.GravshipRaids.Cheat.RotateTemplateSpawn.Label",
                "CheatMenu.GravshipRaids.Cheat.RotateTemplateSpawn.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddAction(context => GravshipRaidsReflection.RotateTemplateSpawn()));
        }

        private static void RegisterSpawnTemplateCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.SpawnTemplate",
                "CheatMenu.GravshipRaids.Cheat.SpawnTemplate.Label",
                "CheatMenu.GravshipRaids.Cheat.SpawnTemplate.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddWindow(OpenTemplateSelectionWindow)
                    .AddTool(
                        SpawnSelectedTemplateAtTarget,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.GravshipRaids.SpawnTemplate.Message.SelectCell"));
        }

        private static void RegisterTestLandingSearchCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.TestLandingSearch",
                "CheatMenu.GravshipRaids.Cheat.TestLandingSearch.Label",
                "CheatMenu.GravshipRaids.Cheat.TestLandingSearch.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddAction(context => GravshipRaidsReflection.TestLandingSearch(Find.CurrentMap)));
        }

        private static void RegisterForceGravshipRaidCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.ForceGravshipRaid",
                "CheatMenu.GravshipRaids.Cheat.ForceGravshipRaid.Label",
                "CheatMenu.GravshipRaids.Cheat.ForceGravshipRaid.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddTool(
                        ForceGravshipRaidAtTarget,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.GravshipRaids.ForceGravshipRaid.Message.SelectCell"));
        }

        private static void RegisterForceGravshipRaidWithTemplateCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.ForceGravshipRaidWithTemplate",
                "CheatMenu.GravshipRaids.Cheat.ForceGravshipRaidWithTemplate.Label",
                "CheatMenu.GravshipRaids.Cheat.ForceGravshipRaidWithTemplate.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddWindow(OpenForceTemplateSelectionWindow)
                    .AddTool(
                        ForceGravshipRaidWithTemplateAtTarget,
                        SpawningCheats.CreateCellTargetingParameters,
                        "CheatMenu.GravshipRaids.ForceGravshipRaidWithTemplate.Message.SelectCell"));
        }

        private static void RegisterForceEnemyGravshipDepartureCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.ForceEnemyGravshipDeparture",
                "CheatMenu.GravshipRaids.Cheat.ForceEnemyGravshipDeparture.Label",
                "CheatMenu.GravshipRaids.Cheat.ForceEnemyGravshipDeparture.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddAction(context => GravshipRaidsReflection.ForceEnemyGravshipDeparture(Find.CurrentMap)));
        }

        private static void RegisterCapturePrefabWithTerrainCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.GravshipRaids.CapturePrefabWithTerrain",
                "CheatMenu.GravshipRaids.Cheat.CapturePrefabWithTerrain.Label",
                "CheatMenu.GravshipRaids.Cheat.CapturePrefabWithTerrain.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddAction(StartPrefabCapture));
        }

        private static void StartPrefabCapture(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();
            GravshipRaidsReflection.StartPrefabCaptureWithTerrain();
        }

        private static void OpenTemplateSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            List<Def> templates = GravshipRaidsReflection.GetTemplates();
            if (templates.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.GravshipRaids.SpawnTemplate.Message.NoneAvailable".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new GravshipRaidTemplateSelectionWindow(
                    templates,
                    selectedTemplate =>
                    {
                        context.Set(SelectedTemplateContextKey, selectedTemplate);
                        continueFlow?.Invoke();
                    }));
        }

        private static void SpawnSelectedTemplateAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedTemplateContextKey, out Def selectedTemplate))
            {
                CheatMessageService.Message("CheatMenu.GravshipRaids.SpawnTemplate.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            GravshipRaidsReflection.SpawnTemplateAt(selectedTemplate, Find.CurrentMap, target.Cell);
        }

        private static void ForceGravshipRaidAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            GravshipRaidsReflection.ForceGravshipRaidAt(Find.CurrentMap, target.Cell);
        }

        private static void OpenForceTemplateSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            List<Def> templates = GravshipRaidsReflection.GetTemplates();
            if (templates.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.GravshipRaids.SpawnTemplate.Message.NoneAvailable".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new GravshipRaidTemplateSelectionWindow(
                    templates,
                    selectedTemplate =>
                    {
                        context.Set(SelectedTemplateContextKey, selectedTemplate);
                        continueFlow?.Invoke();
                    },
                    "CheatMenu.GravshipRaids.ForceGravshipRaidWithTemplate.Window.Title"));
        }

        private static void ForceGravshipRaidWithTemplateAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedTemplateContextKey, out Def selectedTemplate))
            {
                CheatMessageService.Message("CheatMenu.GravshipRaids.SpawnTemplate.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            GravshipRaidsReflection.ForceGravshipRaidAt(selectedTemplate, Find.CurrentMap, target.Cell);
        }
    }
}
