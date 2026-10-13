using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnInspirationSelectionWindow : SearchableSelectionWindow<InspirationDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnStartInspiration.SearchField";

        private readonly Action<InspirationDef> onInspirationSelected;
        private readonly List<InspirationDef> allOptions;

        public PawnInspirationSelectionWindow(Action<InspirationDef> onInspirationSelected)
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

        protected override IReadOnlyList<InspirationDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, InspirationDef option)
        {
            bool canOccurNow = CanOccurNow(option);
            string label = option.label;
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
                "CheatMenu.PawnStartInspiration.Window.InfoLine".Translate(option.defName, availability));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(InspirationDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string inspirationLabel = option.label.ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();
            return inspirationLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(InspirationDef option)
        {
            Close();
            onInspirationSelected?.Invoke(option);
        }

        private static List<InspirationDef> BuildInspirationList()
        {
            List<InspirationDef> result = new List<InspirationDef>();
            foreach (InspirationDef inspirationDef in DefDatabase<InspirationDef>.AllDefsListForReading)
            {
                result.Add(inspirationDef);
            }

            return result
                .OrderBy(option => option.defName)
                .ToList();
        }

        private static bool CanOccurNow(InspirationDef inspirationDef)
        {
            Map map = Find.CurrentMap;
            List<Pawn> freeColonists = map.mapPawns.FreeColonists;
            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn pawn = freeColonists[i];
                if (inspirationDef.Worker.InspirationCanOccur(pawn))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
