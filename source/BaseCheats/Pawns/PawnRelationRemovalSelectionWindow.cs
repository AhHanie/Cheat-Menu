using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnRelationRemovalSelectionWindow : SearchableSelectionWindow<DirectPawnRelation>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnRelation.RemoveWindow.SearchField";

        private readonly Pawn sourcePawn;
        private readonly Action<DirectPawnRelation> onRelationSelected;
        private readonly List<DirectPawnRelation> allOptions;

        public PawnRelationRemovalSelectionWindow(Pawn sourcePawn, Action<DirectPawnRelation> onRelationSelected)
            : base(new Vector2(900f, 700f))
        {
            this.sourcePawn = sourcePawn;
            this.onRelationSelected = onRelationSelected;
            allOptions = sourcePawn.relations.DirectRelations
                .ToList();
        }

        protected override string TitleKey => "CheatMenu.PawnRelation.RemoveWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnRelation.RemoveWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnRelation.RemoveWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnRelation.RemoveWindow.SelectButton";

        protected override IReadOnlyList<DirectPawnRelation> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(sourcePawn.LabelShortCap);
        }

        protected override void DrawItemInfo(Rect rect, DirectPawnRelation option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(rect.x, rect.y, rect.width, 24f),
                "CheatMenu.PawnRelation.RemoveWindow.RowLabel".Translate(option.def.LabelCap, option.otherPawn.LabelShortCap));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnRelation.RemoveWindow.InfoLine".Translate(option.otherPawn.def.defName, option.otherPawn.KindLabel));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(DirectPawnRelation option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string relationLabel = option.def.LabelCap.ToString().ToLowerInvariant();
            string relationDefName = option.def.defName.ToLowerInvariant();
            string pawnLabel = option.otherPawn.LabelShortCap.ToString().ToLowerInvariant();
            string pawnKind = option.otherPawn.KindLabel.ToLowerInvariant();

            return relationLabel.Contains(needle)
                || relationDefName.Contains(needle)
                || pawnLabel.Contains(needle)
                || pawnKind.Contains(needle);
        }

        protected override void OnItemSelected(DirectPawnRelation option)
        {
            Close();
            onRelationSelected?.Invoke(option);
        }
    }
}
