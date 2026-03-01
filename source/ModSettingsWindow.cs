using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class ModSettingsWindow
    {
        private static Vector2 toggleDefaultsScrollPosition = Vector2.zero;

        public static void Draw(Rect parent)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(parent);

            listing.CheckboxLabeled(
                "CheatMenu.Settings.SendCheatMessages.Label".Translate(),
                ref ModSettings.SendCheatMessages,
                "CheatMenu.Settings.SendCheatMessages.Tooltip".Translate());

            listing.CheckboxLabeled(
                "CheatMenu.Settings.ClearCachedSearchOnMenuReopen.Label".Translate(),
                ref ModSettings.ClearCachedSearchOnMenuReopen,
                "CheatMenu.Settings.ClearCachedSearchOnMenuReopen.Tooltip".Translate());

            listing.GapLine();
            listing.Label("CheatMenu.Settings.ToggleCheatDefaults.Label".Translate());
            listing.Label("CheatMenu.Settings.ToggleCheatDefaults.Description".Translate());

            IReadOnlyList<ToggleCheatMetadata> toggleCheats = ToggleCheatRegistry.AllCheats;
            if (toggleCheats.Count == 0)
            {
                listing.Label("CheatMenu.Settings.ToggleCheatDefaults.NoneRegistered".Translate());
                listing.End();
                return;
            }

            float toggleRowsHeight = (toggleCheats.Count * 30f) + 12f;
            float outHeight = Mathf.Min(toggleRowsHeight, 320f);
            Rect outRect = listing.GetRect(outHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, toggleRowsHeight);

            Widgets.BeginScrollView(outRect, ref toggleDefaultsScrollPosition, viewRect);
            Listing_Standard toggleListing = new Listing_Standard();
            toggleListing.Begin(viewRect);

            for (int i = 0; i < toggleCheats.Count; i++)
            {
                ToggleCheatMetadata toggleCheat = toggleCheats[i];
                bool defaultEnabled = ModSettings.GetToggleCheatDefaultEnabled(toggleCheat.Key);
                toggleListing.CheckboxLabeled(
                    BuildToggleSettingsLabel(toggleCheat),
                    ref defaultEnabled,
                    toggleCheat.GetDescription());
                ModSettings.SetToggleCheatDefaultEnabled(toggleCheat.Key, defaultEnabled);
            }

            toggleListing.End();
            Widgets.EndScrollView();
            listing.End();
        }

        private static string BuildToggleSettingsLabel(ToggleCheatMetadata toggleCheat)
        {
            string category = toggleCheat.GetCategoryOrDefault();
            string label = toggleCheat.GetLabel();
            return "CheatMenu.Settings.ToggleCheatDefaults.EntryLabel".Translate(category, label).ToString();
        }
    }
}
