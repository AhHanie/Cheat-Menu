using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnRemoveMemoryThoughtSelectionWindow : SearchableSelectionWindow<Thought_Memory>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnRemoveMemoryThought.SearchField";

        private readonly Pawn sourcePawn;
        private readonly Action<Thought_Memory> onThoughtSelected;
        private readonly List<Thought_Memory> allOptions;

        public PawnRemoveMemoryThoughtSelectionWindow(Pawn sourcePawn, IEnumerable<Thought_Memory> memories, Action<Thought_Memory> onThoughtSelected)
            : base(new Vector2(900f, 700f))
        {
            this.sourcePawn = sourcePawn;
            this.onThoughtSelected = onThoughtSelected;
            allOptions = memories
                .OrderBy(memory => memory.LabelCap)
                .ThenBy(memory => memory.def.defName)
                .ToList();
        }

        protected override string TitleKey => "CheatMenu.PawnRemoveMemoryThought.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnRemoveMemoryThought.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnRemoveMemoryThought.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnRemoveMemoryThought.Window.SelectButton";

        protected override IReadOnlyList<Thought_Memory> Options => allOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(sourcePawn.LabelShortCap);
        }

        protected override void DrawItemInfo(Rect rect, Thought_Memory option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnRemoveMemoryThought.Window.InfoLine".Translate(
                    option.def.defName,
                    option.otherPawn != null ? option.otherPawn.LabelShortCap : "CheatMenu.PawnRemoveMemoryThought.Window.NoOtherPawn".Translate().ToString()));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(Thought_Memory option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = option.LabelCap.ToLowerInvariant();
            string defName = option.def.defName.ToLowerInvariant();

            if (label.Contains(needle) || defName.Contains(needle))
            {
                return true;
            }

            if (option.otherPawn == null)
            {
                return false;
            }

            string otherPawnLabel = option.otherPawn.LabelShortCap.ToString().ToLowerInvariant();
            string otherPawnKind = option.otherPawn.KindLabel.ToLowerInvariant();

            return otherPawnLabel.Contains(needle)
                || otherPawnKind.Contains(needle);
        }

        protected override void OnItemSelected(Thought_Memory option)
        {
            Close();
            onThoughtSelected?.Invoke(option);
        }
    }
}
