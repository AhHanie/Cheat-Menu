using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class ShuttleRaidTemplateSelectionWindow : SearchableSelectionWindow<Def>
    {
        private const string SearchControlNameConst = "CheatMenu.GravshipRaids.SpawnShuttleTemplate.SearchField";
        private const string DefaultTitleKey = "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.Title";

        private readonly Action<Def> onSelected;
        private readonly List<Def> options;
        private readonly string titleKey;

        public ShuttleRaidTemplateSelectionWindow(List<Def> options, Action<Def> onSelected, string titleKey = DefaultTitleKey)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
            this.titleKey = titleKey;
        }

        protected override string TitleKey => titleKey;

        protected override string SearchTooltipKey => "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.SelectButton";

        protected override IReadOnlyList<Def> Options => options;

        protected override bool MatchesSearch(Def item, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = item.label.NullOrEmpty() ? string.Empty : item.label.ToLowerInvariant();
            string defName = item.defName.ToLowerInvariant();
            return defName.Contains(needle) || label.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, Def item)
        {
            bool valid = GravshipRaidsReflection.IsValidShuttleTemplate(item);
            bool disabled = GravshipRaidsReflection.IsShuttleTemplateDisabled(item);

            Text.Font = GameFont.Small;
            Color previousColor = GUI.color;
            GUI.color = !valid ? ColorLibrary.RedReadable : (disabled ? ColorLibrary.Yellow : previousColor);
            string primaryLabel = item.defName;
            if (!valid)
            {
                primaryLabel += "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.InvalidSuffix".Translate().ToString();
            }

            if (disabled)
            {
                primaryLabel += "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.DisabledSuffix".Translate().ToString();
            }

            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), primaryLabel);
            GUI.color = previousColor;

            Text.Font = GameFont.Tiny;
            string infoLine = "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.InfoLine".Translate(
                item.label.NullOrEmpty() ? item.defName : item.LabelCap.ToString());
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;

            if (!valid)
            {
                TooltipHandler.TipRegion(rect, "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.InvalidStatus".Translate());
            }
            else if (disabled)
            {
                TooltipHandler.TipRegion(rect, "CheatMenu.GravshipRaids.SpawnShuttleTemplate.Window.DisabledStatus".Translate());
            }
        }

        protected override void OnItemSelected(Def item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
