using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class GeneralSetStuffSelectionWindow : SearchableSelectionWindow<ThingDef>
    {
        private const string SearchControlNameConst = "CheatMenu.General.SetStuff.SearchField";

        private readonly Action<ThingDef> onStuffSelected;
        private readonly List<ThingDef> stuffDefs;

        public GeneralSetStuffSelectionWindow(Action<ThingDef> onStuffSelected)
            : base(new Vector2(980f, 700f))
        {
            this.onStuffSelected = onStuffSelected;
            stuffDefs = BuildStuffDefList();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override string TitleKey => "CheatMenu.General.SetStuff.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.General.SetStuff.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.General.SetStuff.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.General.SetStuff.Window.SelectButton";

        protected override IReadOnlyList<ThingDef> Options => stuffDefs;

        protected override bool MatchesSearch(ThingDef thingDef, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetSafeLabel(thingDef).ToLowerInvariant();
            string defName = thingDef.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, ThingDef thingDef)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), GetSafeLabel(thingDef));

            string categories = GetCategoriesText(thingDef);
            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.General.SetStuff.Window.InfoLine".Translate(thingDef.defName, categories));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, ThingDef thingDef)
        {
            Texture2D icon = thingDef.uiIcon ?? BaseContent.BadTex;
            Color previousColor = GUI.color;
            GUI.color = thingDef.uiIconColor;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        protected override void OnItemSelected(ThingDef thingDef)
        {
            Close();
            onStuffSelected?.Invoke(thingDef);
        }

        private static List<ThingDef> BuildStuffDefList()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(thingDef => thingDef.IsStuff)
                .OrderBy(GetSafeLabel)
                .ThenBy(thingDef => thingDef.defName)
                .ToList();
        }

        private static string GetSafeLabel(ThingDef thingDef)
        {
            return thingDef.label ?? string.Empty;
        }

        private static string GetCategoriesText(ThingDef thingDef)
        {
            if (thingDef.stuffProps?.categories == null || thingDef.stuffProps.categories.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", thingDef.stuffProps.categories.Select(cat => cat.defName));
        }
    }
}
