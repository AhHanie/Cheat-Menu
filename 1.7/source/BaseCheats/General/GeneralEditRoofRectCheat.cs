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
                "CheatMenu.General.EditRoofRect.Label",
                "CheatMenu.General.EditRoofRect.Description",
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
                CheatMessageService.Message("CheatMenu.General.EditRoofRect.Message.NoOptionSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.MainTabsRoot?.EscapeCurrentTab();

            string toolLabel = selectedOption.IsClear
                ? "CheatMenu.General.EditRoofRect.ToolLabel.Clear".Translate()
                : "CheatMenu.General.EditRoofRect.ToolLabel.Make".Translate(selectedOption.DisplayLabel);

            CheatMessageService.Message(
                "CheatMenu.General.EditRoofRect.Message.StartRectTool".Translate(selectedOption.DisplayLabel),
                MessageTypeDefOf.NeutralEvent,
                false);

            DebugToolsGeneral.GenericRectTool(toolLabel, delegate (CellRect rect)
            {
                Map map = Find.CurrentMap;
                int changedCount = 0;
                foreach (IntVec3 cell in rect)
                {
                    map.roofGrid.SetRoof(cell, selectedOption.RoofDef);
                    changedCount++;
                }

                CheatMessageService.Message(
                    "CheatMenu.General.EditRoofRect.Message.Result".Translate(changedCount, selectedOption.DisplayLabel),
                    changedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            });
        }
    }
}

