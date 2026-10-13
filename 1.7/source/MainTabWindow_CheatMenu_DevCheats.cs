using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public partial class MainTabWindow_CheatMenu
    {
        private static List<DevCheatEntry> cachedDevCheats;

        private void DrawDevSearchRow(Rect rect)
        {
            SearchBarWidget.DrawLabeledSearchRow(
                rect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.Window.SearchTooltip",
                DevSearchControlName,
                130f,
                ref devSearchText,
                ref focusDevSearchOnNextDraw);
        }

        private void DrawDevCheatList(Rect outRect)
        {
            List<DevCheatEntry> devCheats = BuildDevCheats();
            if (devCheats.Count == 0)
            {
                Widgets.Label(outRect, "CheatMenu.DevCheats.NoneAvailable".Translate());
                return;
            }

            List<DevCheatEntry> filteredDevCheats = devCheats
                .Where(MatchesDevSearch)
                .ToList();
            if (filteredDevCheats.Count == 0)
            {
                Widgets.Label(outRect, "CheatMenu.Window.NoCheatsMatchingSearch".Translate(devSearchText));
                return;
            }

            float rowHeight = 30f;
            float viewHeight = 8f + (filteredDevCheats.Count * rowHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref devScrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            for (int i = 0; i < filteredDevCheats.Count; i++)
            {
                DevCheatEntry devCheat = filteredDevCheats[i];
                bool enabled = devCheat.GetValue();
                bool previousEnabled = enabled;

                listing.CheckboxLabeled(devCheat.GetLabel(), ref enabled, devCheat.GetDescription());

                if (enabled == previousEnabled)
                {
                    continue;
                }

                devCheat.SetValue(enabled);
                string messageKey = enabled
                    ? "CheatMenu.DevCheats.Message.Enabled"
                    : "CheatMenu.DevCheats.Message.Disabled";

                CheatMessageService.Message(
                    messageKey.Translate(devCheat.GetLabel()),
                    enabled ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                    false);
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private bool MatchesDevSearch(DevCheatEntry devCheat)
        {
            if (devSearchText.NullOrEmpty())
            {
                return true;
            }

            string needle = devSearchText.Trim().ToLowerInvariant();
            if (needle.Length == 0)
            {
                return true;
            }

            return devCheat.GetLabel().ToLowerInvariant().Contains(needle)
                || devCheat.GetDescription().ToLowerInvariant().Contains(needle);
        }

        private static List<DevCheatEntry> BuildDevCheats()
        {
            if (cachedDevCheats != null)
            {
                return cachedDevCheats;
            }

            cachedDevCheats = typeof(DebugSettings)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(bool) && !field.IsLiteral && !field.IsInitOnly)
                .OrderBy(field => field.MetadataToken)
                .Select(
                    field => new DevCheatEntry(
                        field,
                        "CheatMenu.DevCheat." + field.Name + ".Label",
                        "CheatMenu.DevCheat." + field.Name + ".Description"))
                .ToList();

            return cachedDevCheats;
        }

        private sealed class DevCheatEntry
        {
            private readonly FieldInfo field;
            private readonly string labelKey;
            private readonly string descriptionKey;

            public DevCheatEntry(FieldInfo field, string labelKey, string descriptionKey)
            {
                this.field = field;
                this.labelKey = labelKey;
                this.descriptionKey = descriptionKey;
            }

            public bool GetValue()
            {
                return (bool)field.GetValue(null);
            }

            public void SetValue(bool value)
            {
                field.SetValue(null, value);
            }

            public string GetLabel()
            {
                string label = labelKey.Translate();
                return label == labelKey ? field.Name : label;
            }

            public string GetDescription()
            {
                string description = descriptionKey.Translate();
                return description == descriptionKey ? string.Empty : description;
            }
        }
    }
}
