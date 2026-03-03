using Verse;

namespace Cheat_Menu
{
    public static class ToggleCheatsGeneral
    {
        public const string InfinitePowerKey = "CheatMenu.Toggle.InfinitePower";
        public const string InfinitePsyfocusKey = "CheatMenu.Toggle.InfinitePsyfocus";
        public const string SurgeryNeverFailsKey = "CheatMenu.Toggle.SurgeryNeverFails";
        public const string InfiniteDeepDrillingKey = "CheatMenu.Toggle.InfiniteDeepDrilling";
        public const string AlwaysCraftLegendariesKey = "CheatMenu.Toggle.AlwaysCraftLegendaries";
        public const string InstantGrowGrowingZonesKey = "CheatMenu.Toggle.InstantGrowGrowingZones";
        public const string InfiniteOrbitalTradersKey = "CheatMenu.Toggle.InfiniteOrbitalTraders";
        public const string DisableSolarFlaresKey = "CheatMenu.Toggle.DisableSolarFlares";
        public const string DisableLearningSaturationKey = "CheatMenu.Toggle.DisableLearningSaturation";
        public const string DisableSkillDecayKey = "CheatMenu.Toggle.DisableSkillDecay";
        public const string DisableBiosculpterBiotuningKey = "CheatMenu.Toggle.DisableBiosculpterBiotuning";
        public const string FastBiosculptingKey = "CheatMenu.Toggle.FastBiosculpting";

        public static void Register()
        {
            ToggleCheatRegistry.Register(
                InfinitePowerKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfinitePower.Label",
                    "CheatMenu.ToggleCheat.InfinitePower.Description",
                    "CheatMenu.Category.General"));

            if (ModsConfig.IdeologyActive)
            {
                ToggleCheatRegistry.Register(
                InfinitePsyfocusKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfinitePsyfocus.Label",
                    "CheatMenu.ToggleCheat.InfinitePsyfocus.Description",
                    "CheatMenu.Category.General"));
            }
            
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

            ToggleCheatRegistry.Register(
                InstantGrowGrowingZonesKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InstantGrowGrowingZones.Label",
                    "CheatMenu.ToggleCheat.InstantGrowGrowingZones.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                InfiniteOrbitalTradersKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.InfiniteOrbitalTraders.Label",
                    "CheatMenu.ToggleCheat.InfiniteOrbitalTraders.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                DisableSolarFlaresKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.DisableSolarFlares.Label",
                    "CheatMenu.ToggleCheat.DisableSolarFlares.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                DisableLearningSaturationKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.DisableLearningSaturation.Label",
                    "CheatMenu.ToggleCheat.DisableLearningSaturation.Description",
                    "CheatMenu.Category.General"));

            ToggleCheatRegistry.Register(
                DisableSkillDecayKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.DisableSkillDecay.Label",
                    "CheatMenu.ToggleCheat.DisableSkillDecay.Description",
                    "CheatMenu.Category.General"));

            if (ModsConfig.IdeologyActive)
            {
                ToggleCheatRegistry.Register(
                DisableBiosculpterBiotuningKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.DisableBiosculpterBiotuning.Label",
                    "CheatMenu.ToggleCheat.DisableBiosculpterBiotuning.Description",
                    "CheatMenu.Category.General"));
            }
            
            if (ModsConfig.IdeologyActive)
            {
                ToggleCheatRegistry.Register(
                FastBiosculptingKey,
                new ToggleCheatMetadata(
                    "CheatMenu.ToggleCheat.FastBiosculpting.Label",
                    "CheatMenu.ToggleCheat.FastBiosculpting.Description",
                    "CheatMenu.Category.General"));
            }
        }
    }
}
