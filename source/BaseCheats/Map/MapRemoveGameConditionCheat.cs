using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class MapRemoveGameConditionCheat
    {
        private const string SelectedConditionContextKey = "BaseCheats.Map.RemoveGameCondition.SelectedCondition";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.MapRemoveGameCondition",
                "CheatMenu.Cheat.MapRemoveGameCondition.Label",
                "CheatMenu.Cheat.MapRemoveGameCondition.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Map")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap | CheatAllowedGameStates.HasGameCondition)
                    .RequireMap()
                    .AddWindow(OpenConditionSelectionWindow)
                    .AddAction(RemoveSelectedCondition));
        }

        private static void OpenConditionSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            Find.WindowStack.Add(new MapGameConditionSelectionWindow(delegate (GameConditionDef conditionDef)
            {
                context.Set(SelectedConditionContextKey, conditionDef);
                continueFlow?.Invoke();
            }, onlyActiveConditions: true));
        }

        private static void RemoveSelectedCondition(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            GameConditionDef selectedCondition;
            if (!context.TryGet(SelectedConditionContextKey, out selectedCondition))
            {
                CheatMessageService.Message("CheatMenu.MapRemoveGameCondition.Message.NoConditionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            GameCondition activeCondition = map.gameConditionManager.GetActiveCondition(selectedCondition);
            if (activeCondition == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.MapRemoveGameCondition.Message.NotActive".Translate(selectedCondition.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            activeCondition.Duration = 0;
            CheatMessageService.Message(
                "CheatMenu.MapRemoveGameCondition.Message.Result".Translate(selectedCondition.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
