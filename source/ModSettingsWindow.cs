using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static class ModSettingsWindow
    {
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

            listing.End();
        }
    }
}
