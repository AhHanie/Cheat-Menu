using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnRelationDefSelectionWindow : SearchableSelectionWindow<PawnRelationDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnRelation.RelationDefWindow.SearchField";

        private readonly Action<PawnRelationDef> onRelationSelected;
        private readonly List<PawnRelationDef> allOptions;

        public PawnRelationDefSelectionWindow(Action<PawnRelationDef> onRelationSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onRelationSelected = onRelationSelected;
            allOptions = DefDatabase<PawnRelationDef>.AllDefsListForReading
                .Where(def => !def.implied)
                .OrderBy(def => def.label)
                .ThenBy(def => def.defName)
                .ToList();
        }

        protected override string TitleKey => "CheatMenu.PawnRelation.RelationDefWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnRelation.RelationDefWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnRelation.RelationDefWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnRelation.RelationDefWindow.SelectButton";

        protected override IReadOnlyList<PawnRelationDef> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, PawnRelationDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnRelation.RelationDefWindow.InfoLine".Translate(
                    option.defName,
                    option.familyByBloodRelation
                        ? "CheatMenu.PawnRelation.Common.Yes".Translate()
                        : "CheatMenu.PawnRelation.Common.No".Translate()));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(PawnRelationDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = option.LabelCap.ToString().ToLowerInvariant();
            string defName = option.defName.ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(PawnRelationDef option)
        {
            Close();
            onRelationSelected?.Invoke(option);
        }
    }
}
