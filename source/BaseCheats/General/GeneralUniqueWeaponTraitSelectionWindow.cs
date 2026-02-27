using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class GeneralUniqueWeaponTraitSelectionWindow : SearchableSelectionWindow<WeaponTraitDef>
    {
        private readonly Action<WeaponTraitDef> onTraitSelected;
        private readonly List<WeaponTraitDef> traits;
        private readonly string titleKey;
        private readonly string searchTooltipKey;
        private readonly string searchControlName;
        private readonly string noMatchesKey;
        private readonly string selectButtonKey;

        public GeneralUniqueWeaponTraitSelectionWindow(
            List<WeaponTraitDef> traits,
            Action<WeaponTraitDef> onTraitSelected,
            string titleKey,
            string searchTooltipKey,
            string noMatchesKey,
            string selectButtonKey,
            string searchControlName)
            : base(new Vector2(860f, 700f))
        {
            this.traits = traits;
            this.onTraitSelected = onTraitSelected;
            this.titleKey = titleKey;
            this.searchTooltipKey = searchTooltipKey;
            this.noMatchesKey = noMatchesKey;
            this.selectButtonKey = selectButtonKey;
            this.searchControlName = searchControlName;
        }

        protected override string TitleKey => titleKey;

        protected override string SearchTooltipKey => searchTooltipKey;

        protected override string SearchControlName => searchControlName;

        protected override string NoMatchesKey => noMatchesKey;

        protected override string SelectButtonKey => selectButtonKey;

        protected override IReadOnlyList<WeaponTraitDef> Options => traits;

        protected override bool MatchesSearch(WeaponTraitDef traitDef, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = traitDef.label.ToLowerInvariant();
            string defName = traitDef.defName.ToLowerInvariant();
            return label.Contains(needle) || defName.Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, WeaponTraitDef traitDef)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), traitDef.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.General.UniqueWeaponTrait.Window.InfoLine".Translate(traitDef.defName));
            Text.Font = GameFont.Small;
        }

        protected override void OnItemSelected(WeaponTraitDef traitDef)
        {
            Close();
            onTraitSelected?.Invoke(traitDef);
        }
    }
}
