using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class IncidentDropPodRaidFactionOption
    {
        public IncidentDropPodRaidFactionOption(Faction faction, bool canBeGroupSource)
        {
            Faction = faction;
            CanBeGroupSource = canBeGroupSource;
        }

        public Faction Faction { get; }

        public bool CanBeGroupSource { get; }
    }

    public class IncidentDropPodRaidFactionSelectionWindow : SearchableSelectionWindow<IncidentDropPodRaidFactionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.Incidents.DropPodRaidFaction.SearchField";

        private readonly Action<IncidentDropPodRaidFactionOption> onOptionSelected;
        private readonly List<IncidentDropPodRaidFactionOption> options;

        public IncidentDropPodRaidFactionSelectionWindow(
            List<IncidentDropPodRaidFactionOption> options,
            Action<IncidentDropPodRaidFactionOption> onOptionSelected)
            : base(new Vector2(760f, 680f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.options = options;
            this.onOptionSelected = onOptionSelected;
        }

        protected override string TitleKey => "CheatMenu.Incidents.DropPodRaidFactionWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.Incidents.DropPodRaidFactionWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Incidents.DropPodRaidFactionWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Incidents.DropPodRaidFactionWindow.SelectButton";

        protected override IReadOnlyList<IncidentDropPodRaidFactionOption> Options => options;

        protected override bool MatchesSearch(IncidentDropPodRaidFactionOption option, string needle)
        {
            if (needle.NullOrEmpty())
            {
                return true;
            }

            string label = option.Faction.Name.ToLowerInvariant();
            string defName = option.Faction.def.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, IncidentDropPodRaidFactionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.Faction.Name);

            Text.Font = GameFont.Tiny;
            string availability = option.CanBeGroupSource
                ? "CheatMenu.Incidents.Window.Available".Translate().ToString()
                : "CheatMenu.Incidents.Window.NotAvailable".Translate().ToString();
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.Incidents.DropPodRaidFactionWindow.InfoLine".Translate(option.Faction.def.defName, availability));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(IncidentDropPodRaidFactionOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }
    }
}
