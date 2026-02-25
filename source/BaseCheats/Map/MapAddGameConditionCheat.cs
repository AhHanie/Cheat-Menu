using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    public static class MapAddGameConditionCheat
    {
        private const string SelectedConditionContextKey = "BaseCheats.Map.AddGameCondition.SelectedCondition";
        private const string SelectedPermanentContextKey = "BaseCheats.Map.AddGameCondition.SelectedPermanent";
        private const string SelectedDurationTicksContextKey = "BaseCheats.Map.AddGameCondition.SelectedDurationTicks";
        private const string SelectedDurationLabelContextKey = "BaseCheats.Map.AddGameCondition.SelectedDurationLabel";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapAddGameCondition",
                "CheatMenu.Cheat.MapAddGameCondition.Label",
                "CheatMenu.Cheat.MapAddGameCondition.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddWindow(OpenConditionSelectionWindow)
                    .AddWindow(OpenDurationSelectionWindow)
                    .AddAction(AddSelectedCondition));
        }

        private static void OpenConditionSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new MapGameConditionSelectionWindow(delegate (GameConditionDef conditionDef)
            {
                context.Set(SelectedConditionContextKey, conditionDef);
                continueFlow?.Invoke();
            }, onlyActiveConditions: false));
        }

        private static void OpenDurationSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            GameConditionDef conditionDef;
            if (!context.TryGet(SelectedConditionContextKey, out conditionDef))
            {
                CheatMessageService.Message("CheatMenu.MapAddGameCondition.Message.NoConditionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(new MapGameConditionDurationSelectionWindow(conditionDef, delegate (MapGameConditionDurationOption option)
            {
                context.Set(SelectedPermanentContextKey, option.IsPermanent);
                context.Set(SelectedDurationTicksContextKey, option.DurationTicks);
                context.Set(SelectedDurationLabelContextKey, option.DisplayLabel);
                continueFlow?.Invoke();
            }));
        }

        private static void AddSelectedCondition(CheatExecutionContext context)
        {
            GameConditionDef conditionDef;
            if (!context.TryGet(SelectedConditionContextKey, out conditionDef))
            {
                CheatMessageService.Message("CheatMenu.MapAddGameCondition.Message.NoConditionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!context.TryGet(SelectedPermanentContextKey, out bool isPermanent))
            {
                CheatMessageService.Message("CheatMenu.MapAddGameCondition.Message.NoDurationSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            int durationTicks = 0;
            if (!isPermanent && !context.TryGet(SelectedDurationTicksContextKey, out durationTicks))
            {
                CheatMessageService.Message("CheatMenu.MapAddGameCondition.Message.NoDurationSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            GameCondition gameCondition = GameConditionMaker.MakeCondition(conditionDef);

            if (isPermanent)
            {
                gameCondition.Permanent = true;
            }
            else
            {
                gameCondition.Duration = durationTicks;
            }

            if (!TryGetCurrentConditionManager(out GameConditionManager manager, out string targetLabelKey))
            {
                CheatMessageService.Message("CheatMenu.MapAddGameCondition.Message.NoTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            manager.RegisterCondition(gameCondition);

            string durationLabel = context.Get<string>(
                SelectedDurationLabelContextKey,
                isPermanent
                    ? "CheatMenu.MapAddGameCondition.DurationWindow.PermanentOption".Translate().ToString()
                    : durationTicks.ToStringTicksToPeriod());

            CheatMessageService.Message(
                "CheatMenu.MapAddGameCondition.Message.Result".Translate(conditionDef.LabelCap, durationLabel, targetLabelKey.Translate()),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static bool TryGetCurrentConditionManager(out GameConditionManager manager, out string targetLabelKey)
        {
            Map map = Find.CurrentMap;
            if (map?.gameConditionManager != null)
            {
                manager = map.gameConditionManager;
                targetLabelKey = "CheatMenu.MapGameCondition.Target.Map";
                return true;
            }

            manager = Find.World.GameConditionManager;
            targetLabelKey = "CheatMenu.MapGameCondition.Target.World";
            return true;
        }
    }
}
