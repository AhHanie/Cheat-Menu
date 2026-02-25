using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class AbilitySelectionOption
    {
        public AbilitySelectionOption(bool isAll, AbilityDef abilityDef, string displayLabel)
        {
            IsAll = isAll;
            AbilityDef = abilityDef;
            DisplayLabel = displayLabel;
        }

        public bool IsAll { get; }

        public AbilityDef AbilityDef { get; }

        public string DisplayLabel { get; }
    }

    public class PawnAbilitySelectionWindow : SearchableSelectionWindow<AbilitySelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.PawnGiveAbility.SearchField";
        private const string AllAlias = "all";
        private readonly Action<AbilitySelectionOption> onAbilitySelected;
        private readonly List<AbilitySelectionOption> allOptions;

        public PawnAbilitySelectionWindow(Action<AbilitySelectionOption> onAbilitySelected)
            : base(new Vector2(860f, 700f))
        {
            this.onAbilitySelected = onAbilitySelected;
            allOptions = BuildAbilityList();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override string TitleKey => "CheatMenu.PawnGiveAbility.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.PawnGiveAbility.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.PawnGiveAbility.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.PawnGiveAbility.Window.SelectButton";

        protected override IReadOnlyList<AbilitySelectionOption> Options => allOptions;

        protected override void DrawItemInfo(Rect rect, AbilitySelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.DisplayLabel);

            string defName = option.IsAll
                ? "CheatMenu.PawnGiveAbility.Window.AllDefName".Translate().ToString()
                : (option.AbilityDef?.defName ?? string.Empty);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.PawnGiveAbility.Window.InfoLine".Translate(defName));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, AbilitySelectionOption option)
        {
            if (option.IsAll)
            {
                Widgets.DrawBoxSolid(iconRect, new Color(0.23f, 0.23f, 0.23f));
                TextAnchor previousAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(iconRect, "*");
                Text.Anchor = previousAnchor;
                return;
            }

            Texture2D icon = option.AbilityDef.uiIcon;
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        protected override bool MatchesSearch(AbilitySelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string displayLabel = option.DisplayLabel.ToLowerInvariant();
            string defName = option.AbilityDef.defName.ToLowerInvariant();

            if (option.IsAll)
            {
                
                return displayLabel.Contains(needle) || AllAlias.Contains(needle);
            }

            return displayLabel.Contains(needle) || defName.Contains(needle);
        }

        protected override void OnItemSelected(AbilitySelectionOption option)
        {
            Close();
            onAbilitySelected?.Invoke(option);
        }

        private static List<AbilitySelectionOption> BuildAbilityList()
        {
            List<AbilitySelectionOption> result = new List<AbilitySelectionOption>
            {
                new AbilitySelectionOption(
                    isAll: true,
                    abilityDef: null,
                    displayLabel: "CheatMenu.PawnGiveAbility.Window.AllOption".Translate())
            };

            List<AbilitySelectionOption> abilityOptions = new List<AbilitySelectionOption>();
            foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading)
            {
                abilityOptions.Add(new AbilitySelectionOption(false, abilityDef, abilityDef.LabelCap));
            }

            result.AddRange(
                abilityOptions
                    .OrderBy(option => option.DisplayLabel)
                    .ThenBy(option => option.AbilityDef.defName));

            return result;
        }
    }
}
