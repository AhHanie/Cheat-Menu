namespace Cheat_Menu
{
    public static class ToggleCheatsGeneral
    {
        public const string InfinitePowerKey = "CheatMenu.Toggle.InfinitePower";
        public const string InfinitePsyfocusKey = "CheatMenu.Toggle.InfinitePsyfocus";
        public const string SurgeryNeverFailsKey = "CheatMenu.Toggle.SurgeryNeverFails";
        public const string InfiniteDeepDrillingKey = "CheatMenu.Toggle.InfiniteDeepDrilling";
        public const string AlwaysCraftLegendariesKey = "CheatMenu.Toggle.AlwaysCraftLegendaries";

        public static void Register()
        {
            ToggleCheatRegistry.Register(
                InfinitePowerKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfinitePower.Label",
                    "CheatMenu.ToggleCheat.InfinitePower.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                InfinitePsyfocusKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfinitePsyfocus.Label",
                    "CheatMenu.ToggleCheat.InfinitePsyfocus.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                SurgeryNeverFailsKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.SurgeryNeverFails.Label",
                    "CheatMenu.ToggleCheat.SurgeryNeverFails.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                InfiniteDeepDrillingKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfiniteDeepDrilling.Label",
                    "CheatMenu.ToggleCheat.InfiniteDeepDrilling.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                AlwaysCraftLegendariesKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.AlwaysCraftLegendaries.Label",
                    "CheatMenu.ToggleCheat.AlwaysCraftLegendaries.Description",
                    "CheatMenu.Category.General"));
        }
    }
}
