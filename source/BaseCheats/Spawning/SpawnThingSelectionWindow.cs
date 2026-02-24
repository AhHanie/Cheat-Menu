using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class SpawnThingSelectionWindow : SearchableSelectionWindow<ThingDef>
    {
        private const string SearchControlNameConst = "CheatMenu.SpawnThing.SearchField";

        private readonly Action<ThingDef> onThingSelected;
        private readonly List<ThingDef> allThingDefs;

        public SpawnThingSelectionWindow(Action<ThingDef> onThingSelected)
            : base(new Vector2(980f, 700f))
        {
            this.onThingSelected = onThingSelected;
            allThingDefs = BuildThingDefList();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override float SelectButtonWidth => 86f;

        protected override string TitleKey => "CheatMenu.SpawnThing.SelectWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.SpawnThing.SelectWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SpawnThing.SelectWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SpawnThing.SelectWindow.SelectButton";

        protected override IReadOnlyList<ThingDef> Options => allThingDefs;

        protected override void DrawItemInfo(Rect rect, ThingDef thingDef)
        {
            string label = GetSafeLabel(thingDef);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.SpawnThing.SelectWindow.DefNameLine".Translate(thingDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, ThingDef thingDef)
        {
            Texture2D icon = GetIcon(thingDef);
            if (icon == null)
            {
                Widgets.DrawBoxSolid(iconRect, new Color(0.2f, 0.2f, 0.2f));
                Widgets.Label(iconRect, "?");
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = thingDef?.uiIconColor ?? Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private static Texture2D GetIcon(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return BaseContent.BadTex;
            }

            if (thingDef.uiIcon != null)
            {
                return thingDef.uiIcon;
            }

            return BaseContent.BadTex;
        }

        protected override bool MatchesSearch(ThingDef thingDef, string needle)
        {
            if (thingDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetSafeLabel(thingDef).ToLowerInvariant();
            string defName = thingDef.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(ThingDef thingDef)
        {
            Close();
            onThingSelected?.Invoke(thingDef);
        }

        private static List<ThingDef> BuildThingDefList()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(IsSelectableThingDef)
                .OrderBy(GetSafeLabel)
                .ThenBy((ThingDef thingDef) => thingDef.defName)
                .ToList();
        }

        private static bool IsSelectableThingDef(ThingDef thingDef)
        {
            // Match RimWorld's debug spawn item filtering behavior.
            return DebugThingPlaceHelper.IsDebugSpawnable(thingDef);
        }

        private static string GetSafeLabel(ThingDef thingDef)
        {
            return thingDef?.label ?? string.Empty;
        }
    }
}
