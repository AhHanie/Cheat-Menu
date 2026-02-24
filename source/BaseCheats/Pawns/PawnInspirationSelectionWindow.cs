using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class InspirationSelectionOption
    {
        public InspirationSelectionOption(InspirationDef inspirationDef, string displayLabel)
        {
            InspirationDef = inspirationDef;
            DisplayLabel = displayLabel ?? string.Empty;
        }

        public InspirationDef InspirationDef { get; }

        public string DisplayLabel { get; }
    }

    public sealed class PawnInspirationSelectionWindow : SearchableSelectionWindow<InspirationSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnStartInspiration.SearchField";

        private readonly Action<InspirationSelectionOption> onInspirationSelected;
        private readonly List<InspirationSelectionOption> allOptions;

        public PawnInspirationSelectionWindow(Action<InspirationSelectionOption> onInspirationSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onInspirationSelected = onInspirationSelected;
            allOptions = BuildInspirationList();
        }

        protected override string TitleKey => "CheatMenu.PawnStartInspiration.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnStartInspiration.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnStartInspiration.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnStartInspiration.Window.SelectButton";

        protected override IReadOnlyList<InspirationSelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, InspirationSelectionOption option)
        {
            InspirationDef inspirationDef = option?.InspirationDef;
            if (inspirationDef == null)
            {
                return;
            }

            bool canOccurNow = CanOccurNow(inspirationDef);
            string label = option.DisplayLabel;
            if (!canOccurNow)
            {
                label += " [NO]";
            }

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            string availability = canOccurNow
                ? "CheatMenu.PawnStartInspiration.Window.Available".Translate()
                : "CheatMenu.PawnStartInspiration.Window.NotAvailable".Translate();

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnStartInspiration.Window.InfoLine".Translate(inspirationDef.defName, availability));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(InspirationSelectionOption option, string needle)
        {
            InspirationDef inspirationDef = option?.InspirationDef;
            if (inspirationDef == null)
            {
                return false;
            }

            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = (option.DisplayLabel ?? string.Empty).ToLowerInvariant();
            string inspirationLabel = (inspirationDef.label ?? string.Empty).ToLowerInvariant();
            string defName = inspirationDef.defName.ToLowerInvariant();
            return displayLabel.Contains(needle) || inspirationLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(InspirationSelectionOption option)
        {
            Close();
            onInspirationSelected?.Invoke(option);
        }

        private static List<InspirationSelectionOption> BuildInspirationList()
        {
            List<InspirationSelectionOption> result = new List<InspirationSelectionOption>();
            foreach (InspirationDef inspirationDef in DefDatabase<InspirationDef>.AllDefsListForReading)
            {
                if (inspirationDef == null || inspirationDef.Worker == null)
                {
                    continue;
                }

                string label = inspirationDef.label ?? inspirationDef.defName ?? string.Empty;
                result.Add(new InspirationSelectionOption(inspirationDef, label));
            }

            return result
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.InspirationDef.defName)
                .ToList();
        }

        private static bool CanOccurNow(InspirationDef inspirationDef)
        {
            if (inspirationDef?.Worker == null)
            {
                return false;
            }

            Map map = Find.CurrentMap;
            List<Pawn> freeColonists = map?.mapPawns?.FreeColonists;
            if (freeColonists == null || freeColonists.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn pawn = freeColonists[i];
                if (pawn != null && inspirationDef.Worker.InspirationCanOccur(pawn))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
