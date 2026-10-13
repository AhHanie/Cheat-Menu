using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace Cheat_Menu
{
    public class PawnRelationTargetPawnSelectionWindow : SearchableSelectionWindow<Pawn>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnRelation.TargetWindow.SearchField";

        private readonly Pawn sourcePawn;
        private readonly PawnRelationDef relationDef;
        private readonly Action<Pawn> onPawnSelected;
        private readonly List<Pawn> candidates;

        public PawnRelationTargetPawnSelectionWindow(
            Pawn sourcePawn,
            PawnRelationDef relationDef,
            List<Pawn> candidates,
            Action<Pawn> onPawnSelected)
            : base(new Vector2(960f, 700f))
        {
            this.sourcePawn = sourcePawn;
            this.relationDef = relationDef;
            this.candidates = candidates;
            this.onPawnSelected = onPawnSelected;
        }

        protected override string TitleKey => "CheatMenu.PawnRelation.TargetWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnRelation.TargetWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnRelation.TargetWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnRelation.TargetWindow.SelectButton";

        protected override IReadOnlyList<Pawn> Options => candidates;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(sourcePawn.LabelShortCap, relationDef.LabelCap);
        }

        protected override void DrawItemInfo(Rect rect, Pawn option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(
                new Rect(rect.x, rect.y, rect.width, 24f),
                "CheatMenu.PawnRelation.TargetWindow.RowLabel".Translate(option.LabelShortCap, option.KindLabel));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnRelation.TargetWindow.InfoLine".Translate(
                    option.def.defName,
                    option.IsWorldPawn()
                        ? "CheatMenu.PawnRelation.TargetWindow.WorldPawn".Translate()
                        : "CheatMenu.PawnRelation.TargetWindow.MapPawn".Translate()));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(Pawn option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string shortLabel = option.LabelShortCap.ToString().ToLowerInvariant();
            string kindLabel = option.KindLabel.ToLowerInvariant();
            string defName = option.def.defName.ToLowerInvariant();

            return shortLabel.Contains(needle)
                || kindLabel.Contains(needle)
                || defName.Contains(needle);
        }

        protected override void OnItemSelected(Pawn option)
        {
            Close();
            onPawnSelected?.Invoke(option);
        }

        public static List<Pawn> BuildCandidates(Pawn sourcePawn, PawnRelationDef relationDef)
        {
            return PawnsFinder.AllMapsWorldAndTemporary_Alive
                .Where(x => x.RaceProps.IsFlesh || (x.RaceProps.IsMechanoid && x.Faction == Faction.OfPlayer))
                .Where(x => sourcePawn != x
                            && (!relationDef.familyByBloodRelation || x.def == sourcePawn.def)
                            && !sourcePawn.relations.DirectRelationExists(relationDef, x))
                .OrderByDescending(x => x.def == sourcePawn.def)
                .ThenBy(x => x.IsWorldPawn())
                .ThenBy(x => x.LabelShortCap.ToString())
                .ToList();
        }
    }
}
