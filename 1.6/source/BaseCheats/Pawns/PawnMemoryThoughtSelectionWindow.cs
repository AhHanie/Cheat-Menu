using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class PawnMemoryThoughtSelectionWindow : SearchableSelectionWindow<ThoughtDef>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnAddMemoryThought.SearchField";

        private static readonly List<ThoughtDef> cachedMemoryThoughtDefs = BuildMemoryThoughtList();

        private readonly Action<ThoughtDef> onThoughtSelected;

        public PawnMemoryThoughtSelectionWindow(Action<ThoughtDef> onThoughtSelected)
            : base(new Vector2(860f, 700f))
        {
            this.onThoughtSelected = onThoughtSelected;
        }

        protected override string TitleKey => "CheatMenu.PawnAddMemoryThought.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnAddMemoryThought.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnAddMemoryThought.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnAddMemoryThought.Window.SelectButton";

        protected override IReadOnlyList<ThoughtDef> Options => cachedMemoryThoughtDefs;

        protected override void DrawItemInfo(Rect rect, ThoughtDef option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.defName);
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(ThoughtDef option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string defName = option.defName.ToLowerInvariant();

            return defName.Contains(needle);
        }

        protected override void OnItemSelected(ThoughtDef option)
        {
            Close();
            onThoughtSelected?.Invoke(option);
        }

        private static List<ThoughtDef> BuildMemoryThoughtList()
        {
            List<ThoughtDef> result = new List<ThoughtDef>();
            foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                if (thoughtDef.IsMemory)
                {
                    result.Add(thoughtDef);
                }
            }

            return result
                .OrderBy(option => option.label)
                .ThenBy(option => option.defName)
                .ToList();
        }
    }
}
