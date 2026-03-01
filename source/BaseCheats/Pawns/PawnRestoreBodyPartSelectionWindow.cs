using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnRestoreBodyPartSelectionWindow : SearchableSelectionWindow<BodyPartRecord>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnRestoreBodyPart.SearchField";

        private readonly Action<BodyPartRecord> onPartSelected;
        private readonly string pawnLabel;
        private readonly List<BodyPartRecord> allOptions;

        public PawnRestoreBodyPartSelectionWindow(Pawn pawn, Action<BodyPartRecord> onPartSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onPartSelected = onPartSelected;
            pawnLabel = pawn.LabelShortCap;
            allOptions = BuildBodyPartOptions(pawn);
        }

        protected override string TitleKey => "CheatMenu.PawnRestoreBodyPart.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnRestoreBodyPart.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnRestoreBodyPart.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnRestoreBodyPart.Window.SelectButton";

        protected override IReadOnlyList<BodyPartRecord> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(pawnLabel);
        }

        protected override void DrawItemInfo(Rect rect, BodyPartRecord option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), BuildDisplayLabel(option));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnRestoreBodyPart.Window.InfoLine".Translate(option.def.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(BodyPartRecord option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = BuildDisplayLabel(option).ToLowerInvariant();
            string partLabel = option.def.label.ToLowerInvariant();
            string defName = option.def.defName.ToLowerInvariant();

            return displayLabel.Contains(needle)
                || partLabel.Contains(needle)
                || defName.Contains(needle);
        }

        protected override void OnItemSelected(BodyPartRecord option)
        {
            Close();
            onPartSelected?.Invoke(option);
        }

        private static List<BodyPartRecord> BuildBodyPartOptions(Pawn pawn)
        {
            return pawn.health.hediffSet.GetNotMissingParts()
                .OrderBy(option => BuildDisplayLabel(option))
                .ToList();
        }

        private static string BuildDisplayLabel(BodyPartRecord part)
        {
            string displayLabel = part.LabelCap;
            if (part.parent != null)
            {
                displayLabel = displayLabel + " (" + part.parent.LabelCap + ")";
            }

            return displayLabel;
        }
    }
}
