using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralEditRoofRectSelectedOptionContextKey = "BaseCheats.GeneralEditRoofRect.SelectedOption";

        private static void RegisterEditRoofRect()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralEditRoofRect",
                "CheatMenu.Cheat.GeneralEditRoofRect.Label",
                "CheatMenu.Cheat.GeneralEditRoofRect.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenRoofSelectionWindow)
                    .AddAction(StartRoofRectTool));
        }

        private static void OpenRoofSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new GeneralRoofSelectionWindow(delegate (GeneralRoofSelectionOption selectedOption)
            {
                context.Set(GeneralEditRoofRectSelectedOptionContextKey, selectedOption);
                continueFlow?.Invoke();
            }));
        }

        private static void StartRoofRectTool(CheatExecutionContext context)
        {
            GeneralRoofSelectionOption selectedOption;
            if (!context.TryGet(GeneralEditRoofRectSelectedOptionContextKey, out selectedOption) || selectedOption == null)
            {
                CheatMessageService.Message("CheatMenu.GeneralEditRoofRect.Message.NoOptionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.MainTabsRoot?.EscapeCurrentTab();

            string toolLabel = selectedOption.IsClear
                ? "CheatMenu.GeneralEditRoofRect.ToolLabel.Clear".Translate()
                : "CheatMenu.GeneralEditRoofRect.ToolLabel.Make".Translate(selectedOption.DisplayLabel);

            CheatMessageService.Message(
                "CheatMenu.GeneralEditRoofRect.Message.StartRectTool".Translate(selectedOption.DisplayLabel),
                MessageTypeDefOf.NeutralEvent,
                false);

            DebugToolsGeneral.GenericRectTool(toolLabel, delegate (CellRect rect)
            {
                Map map = Find.CurrentMap;
                if (map == null)
                {
                    return;
                }

                int changedCount = 0;
                foreach (IntVec3 cell in rect)
                {
                    if (!cell.InBounds(map))
                    {
                        continue;
                    }

                    map.roofGrid.SetRoof(cell, selectedOption.RoofDef);
                    changedCount++;
                }

                CheatMessageService.Message(
                    "CheatMenu.GeneralEditRoofRect.Message.Result".Translate(changedCount, selectedOption.DisplayLabel),
                    changedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }
    }
}
