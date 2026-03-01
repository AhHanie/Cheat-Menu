using Verse;

namespace Cheat_Menu
{
    public static class ToggleCheatsNeeds
    {
        public const string InfiniteJoyKey = "CheatMenu.Toggle.InfiniteJoy";
        public const string InfiniteFoodKey = "CheatMenu.Toggle.InfiniteFood";
        public const string InfiniteRestKey = "CheatMenu.Toggle.InfiniteRest";
        public const string InfiniteIndoorsKey = "CheatMenu.Toggle.InfiniteIndoors";
        public const string InfiniteMechEnergyKey = "CheatMenu.Toggle.InfiniteMechEnergy";
        public const string InfiniteBeautyKey = "CheatMenu.Toggle.InfiniteBeauty";
        public const string InfiniteComfortKey = "CheatMenu.Toggle.InfiniteComfort";
        public const string InfiniteOutdoorsKey = "CheatMenu.Toggle.InfiniteOutdoors";
        public const string InfiniteMoodKey = "CheatMenu.Toggle.InfiniteMood";

        public static void Register()
        {
            RegisterNeedToggle(
                InfiniteJoyKey,
                "CheatMenu.ToggleCheat.InfiniteJoy.Label",
                "CheatMenu.ToggleCheat.InfiniteJoy.Description");

            RegisterNeedToggle(
                InfiniteFoodKey,
                "CheatMenu.ToggleCheat.InfiniteFood.Label",
                "CheatMenu.ToggleCheat.InfiniteFood.Description");

            RegisterNeedToggle(
                InfiniteRestKey,
                "CheatMenu.ToggleCheat.InfiniteRest.Label",
                "CheatMenu.ToggleCheat.InfiniteRest.Description");

            RegisterNeedToggle(
                InfiniteIndoorsKey,
                "CheatMenu.ToggleCheat.InfiniteIndoors.Label",
                "CheatMenu.ToggleCheat.InfiniteIndoors.Description");

            RegisterNeedToggle(
                InfiniteBeautyKey,
                "CheatMenu.ToggleCheat.InfiniteBeauty.Label",
                "CheatMenu.ToggleCheat.InfiniteBeauty.Description");

            RegisterNeedToggle(
                InfiniteComfortKey,
                "CheatMenu.ToggleCheat.InfiniteComfort.Label",
                "CheatMenu.ToggleCheat.InfiniteComfort.Description");

            RegisterNeedToggle(
                InfiniteOutdoorsKey,
                "CheatMenu.ToggleCheat.InfiniteOutdoors.Label",
                "CheatMenu.ToggleCheat.InfiniteOutdoors.Description");

            RegisterNeedToggle(
                InfiniteMoodKey,
                "CheatMenu.ToggleCheat.InfiniteMood.Label",
                "CheatMenu.ToggleCheat.InfiniteMood.Description");

            if (ModsConfig.BiotechActive)
            {
                RegisterNeedToggle(
                    InfiniteMechEnergyKey,
                    "CheatMenu.ToggleCheat.InfiniteMechEnergy.Label",
                    "CheatMenu.ToggleCheat.InfiniteMechEnergy.Description");
            }
        }

        private static void RegisterNeedToggle(string key, string labelKey, string descriptionKey)
        {
            ToggleCheatRegistry.Register(
                key,
                new ToggleCheatMetadata(
                    labelKey,
                    descriptionKey,
                    "CheatMenu.ToggleCheat.Category.Needs"));
        }
    }
}
