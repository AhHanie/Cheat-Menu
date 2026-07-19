using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class GravshipRaidTemplateSelectionWindow : SearchableSelectionWindow<Def>
    {
        private const string SearchControlNameConst = "CheatMenu.GravshipRaids.SpawnTemplate.SearchField";
        private readonly Action<Def> onSelected;
        private readonly List<Def> options;

        public GravshipRaidTemplateSelectionWindow(List<Def> options, Action<Def> onSelected)
            : base(new Vector2(860f, 700f))
        {
            this.options = options;
            this.onSelected = onSelected;
        }

        protected override string TitleKey => "CheatMenu.GravshipRaids.SpawnTemplate.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.GravshipRaids.SpawnTemplate.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.GravshipRaids.SpawnTemplate.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.GravshipRaids.SpawnTemplate.Window.SelectButton";

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
            bool valid = !item.ConfigErrors().Any();

            Text.Font = GameFont.Small;
            Color previousColor = GUI.color;
            GUI.color = valid ? previousColor : ColorLibrary.RedReadable;
            string primaryLabel = valid
                ? item.defName
                : item.defName + "CheatMenu.GravshipRaids.SpawnTemplate.Window.InvalidSuffix".Translate().ToString();
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), primaryLabel);
            GUI.color = previousColor;

            Text.Font = GameFont.Tiny;
            string infoLine = "CheatMenu.GravshipRaids.SpawnTemplate.Window.InfoLine".Translate(
                item.label.NullOrEmpty() ? item.defName : item.LabelCap.ToString());
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 20f), infoLine);
            Text.Font = GameFont.Small;

            if (!valid)
            {
                TooltipHandler.TipRegion(rect, "CheatMenu.GravshipRaids.SpawnTemplate.Window.InvalidStatus".Translate());
            }
        }

        protected override void OnItemSelected(Def item)
        {
            Close();
            onSelected?.Invoke(item);
        }
    }
}
