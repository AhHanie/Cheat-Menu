using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IdeologyRelicSelectionWindow : SearchableSelectionWindow<Precept_Relic>
    {
        private const string SearchControlNameConst = "CheatMenu.Ideology.SpawnRelic.SearchField";

        private readonly Action<Precept_Relic> onRelicSelected;
        private readonly List<Precept_Relic> relicPrecepts;

        public IdeologyRelicSelectionWindow(List<Precept_Relic> relicPrecepts, Action<Precept_Relic> onRelicSelected)
            : base(new Vector2(900f, 700f))
        {
            this.relicPrecepts = relicPrecepts;
            this.onRelicSelected = onRelicSelected;
        }

        protected override string TitleKey => "CheatMenu.Ideology.SpawnRelic.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.Ideology.SpawnRelic.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Ideology.SpawnRelic.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Ideology.SpawnRelic.Window.SelectButton";

        protected override IReadOnlyList<Precept_Relic> Options => relicPrecepts;

        protected override void DrawItemInfo(Rect rect, Precept_Relic option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), GetDisplayLabel(option));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Ideology.SpawnRelic.Window.InfoLine".Translate(option.def.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(Precept_Relic option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetDisplayLabel(option).ToLowerInvariant();
            string defName = option.def.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(Precept_Relic option)
        {
            Close();
            onRelicSelected?.Invoke(option);
        }

        public static List<Precept_Relic> BuildRelicPreceptList()
        {
            Ideo playerIdeo = Faction.OfPlayer?.ideos?.PrimaryIdeo;
            if (playerIdeo == null)
            {
                return new List<Precept_Relic>();
            }

            return playerIdeo.PreceptsListForReading
                .OfType<Precept_Relic>()
                .OrderBy(GetDisplayLabel)
                .ThenBy(relic => relic.def.defName)
                .ToList();
        }

        private static string GetDisplayLabel(Precept_Relic relicPrecept)
        {
            return relicPrecept.ToString();
        }
    }
}
