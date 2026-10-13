using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class GeneralSetColorOption
    {
        public GeneralSetColorOption(string label, string info, Color color, Texture2D icon, bool randomizeOnUse)
        {
            Label = label;
            Info = info;
            Color = color;
            Icon = icon;
            RandomizeOnUse = randomizeOnUse;
        }

        public string Label { get; }

        public string Info { get; }

        public Color Color { get; }

        public Texture2D Icon { get; }

        public bool RandomizeOnUse { get; }

        public Color ResolveColor()
        {
            return RandomizeOnUse ? GenColor.RandomColorOpaque() : Color;
        }
    }

    public sealed class GeneralSetColorSelectionWindow : SearchableSelectionWindow<GeneralSetColorOption>
    {
        private const string SearchControlNameConst = "CheatMenu.General.SetColor.SearchField";

        private readonly Action<GeneralSetColorOption> onOptionSelected;
        private readonly List<GeneralSetColorOption> options;

        public GeneralSetColorSelectionWindow(Action<GeneralSetColorOption> onOptionSelected)
            : base(new Vector2(860f, 700f), rowHeight: 56f, rowSpacing: 4f)
        {
            this.onOptionSelected = onOptionSelected;
            options = BuildOptions();
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 32f;

        protected override string TitleKey => "CheatMenu.General.SetColor.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.General.SetColor.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.General.SetColor.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.General.SetColor.Window.SelectButton";

        protected override IReadOnlyList<GeneralSetColorOption> Options => options;

        protected override bool MatchesSearch(GeneralSetColorOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            return option.Label.ToLowerInvariant().Contains(needle)
                || option.Info.ToLowerInvariant().Contains(needle);
        }

        protected override void DrawItemInfo(Rect rect, GeneralSetColorOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), option.Label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.General.SetColor.Window.InfoLine".Translate(option.Info));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect rect, GeneralSetColorOption option)
        {
            Widgets.DrawBoxSolid(rect, option.Color);

            Rect iconRect = rect.ContractedBy(3f);
            Texture2D icon = option.Icon;
            Color previousColor = GUI.color;
            GUI.color = option.Color;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        protected override void OnItemSelected(GeneralSetColorOption option)
        {
            Close();
            onOptionSelected?.Invoke(option);
        }

        private static List<GeneralSetColorOption> BuildOptions()
        {
            List<GeneralSetColorOption> list = new List<GeneralSetColorOption>
            {
                new GeneralSetColorOption(
                    "CheatMenu.General.SetColor.Window.Option.Random".Translate().ToString(),
                    "CheatMenu.General.SetColor.Window.Source.Random".Translate().ToString(),
                    Color.white,
                    BaseContent.WhiteTex,
                    randomizeOnUse: true)
            };

            foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading)
            {
                if (!ideo.classicMode && ideo.Icon != BaseContent.BadTex)
                {
                    list.Add(new GeneralSetColorOption(
                        ideo.name,
                        "CheatMenu.General.SetColor.Window.Source.Ideology".Translate().ToString(),
                        ideo.Color,
                        ideo.Icon,
                        randomizeOnUse: false));
                }
            }

            foreach (ColorDef colorDef in DefDatabase<ColorDef>.AllDefsListForReading)
            {
                list.Add(new GeneralSetColorOption(
                    colorDef.defName,
                    "CheatMenu.General.SetColor.Window.Source.ColorDef".Translate().ToString(),
                    colorDef.color,
                    BaseContent.WhiteTex,
                    randomizeOnUse: false));
            }

            return list;
        }
    }
}
