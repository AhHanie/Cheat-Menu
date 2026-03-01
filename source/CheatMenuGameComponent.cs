using System;
using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public class CheatMenuGameComponent : GameComponent
    {
        private Dictionary<string, bool> enabledByKey =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public CheatMenuGameComponent(Game game)
        {
        }

        public bool IsEnabled(string key)
        {
            if (enabledByKey.TryGetValue(key, out bool enabled))
            {
                return enabled;
            }

            enabled = ModSettings.GetToggleCheatDefaultEnabled(key);
            enabledByKey[key] = enabled;
            return enabled;
        }

        public void SetEnabled(string key, bool value)
        {
            enabledByKey[key] = value;
        }

        public override void StartedNewGame()
        {
            InitializeFromDefaults();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref enabledByKey, "toggleCheatEnabledByKey", LookMode.Value, LookMode.Value);

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            if (enabledByKey == null)
            {
                enabledByKey = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            }

            InitializeMissingFromDefaults();
        }

        private void InitializeFromDefaults()
        {
            enabledByKey.Clear();

            IReadOnlyList<ToggleCheatMetadata> cheats = ToggleCheatRegistry.AllCheats;
            for (int i = 0; i < cheats.Count; i++)
            {
                ToggleCheatMetadata cheat = cheats[i];
                enabledByKey[cheat.Key] = ModSettings.GetToggleCheatDefaultEnabled(cheat.Key);
            }
        }

        private void InitializeMissingFromDefaults()
        {
            IReadOnlyList<ToggleCheatMetadata> cheats = ToggleCheatRegistry.AllCheats;
            for (int i = 0; i < cheats.Count; i++)
            {
                ToggleCheatMetadata cheat = cheats[i];
                if (enabledByKey.ContainsKey(cheat.Key))
                {
                    continue;
                }

                enabledByKey[cheat.Key] = ModSettings.GetToggleCheatDefaultEnabled(cheat.Key);
            }
        }
    }
}
