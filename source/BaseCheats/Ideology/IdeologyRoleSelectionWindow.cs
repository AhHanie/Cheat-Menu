using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class IdeologyRoleSelectionOption
    {
        public IdeologyRoleSelectionOption(Precept_Role role, bool clearCurrentRole)
        {
            Role = role;
            ClearCurrentRole = clearCurrentRole;
        }

        public Precept_Role Role { get; }

        public bool ClearCurrentRole { get; }
    }

    public class IdeologyRoleSelectionWindow : SearchableSelectionWindow<IdeologyRoleSelectionOption>
    {
        private const string SearchControlNameConst = "CheatMenu.Ideology.SetRole.SearchField";

        private readonly Action<IdeologyRoleSelectionOption> onRoleSelected;
        private readonly Pawn pawn;
        private readonly List<IdeologyRoleSelectionOption> roleOptions;

        public IdeologyRoleSelectionWindow(Pawn pawn, List<IdeologyRoleSelectionOption> roleOptions, Action<IdeologyRoleSelectionOption> onRoleSelected)
            : base(new Vector2(900f, 700f))
        {
            this.pawn = pawn;
            this.roleOptions = roleOptions;
            this.onRoleSelected = onRoleSelected;
        }

        protected override string TitleKey => "CheatMenu.Ideology.SetRole.Window.Title";

        protected override string SearchTooltipKey => "CheatMenu.Ideology.SetRole.Window.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.Ideology.SetRole.Window.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.Ideology.SetRole.Window.SelectButton";

        protected override IReadOnlyList<IdeologyRoleSelectionOption> Options => roleOptions;

        protected override TaggedString GetTitleText()
        {
            return TitleKey.Translate(pawn.LabelShortCap);
        }

        protected override void DrawItemInfo(Rect rect, IdeologyRoleSelectionOption option)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), GetDisplayLabel(option));

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                option.ClearCurrentRole
                    ? "CheatMenu.Ideology.SetRole.Window.NoneInfo".Translate()
                    : "CheatMenu.Ideology.SetRole.Window.InfoLine".Translate(option.Role.def.defName));
            Text.Font = GameFont.Small;
        }

        protected override bool MatchesSearch(IdeologyRoleSelectionOption option, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetDisplayLabel(option).ToLowerInvariant();
            if (label.Contains(needle))
            {
                return true;
            }

            if (option.ClearCurrentRole)
            {
                return false;
            }

            return option.Role.def.defName.ToLowerInvariant().Contains(needle);
        }

        protected override void OnItemSelected(IdeologyRoleSelectionOption option)
        {
            Close();
            onRoleSelected?.Invoke(option);
        }

        public static List<IdeologyRoleSelectionOption> BuildRoleOptions(Pawn pawn)
        {
            List<IdeologyRoleSelectionOption> options = new List<IdeologyRoleSelectionOption>();
            Precept_Role currentRole = pawn.Ideo.GetRole(pawn);

            for (int i = 0; i < pawn.Ideo.cachedPossibleRoles.Count; i++)
            {
                Precept_Role role = pawn.Ideo.cachedPossibleRoles[i];
                if (role == currentRole)
                {
                    continue;
                }

                options.Add(new IdeologyRoleSelectionOption(role, clearCurrentRole: false));
            }

            if (currentRole != null)
            {
                options.Add(new IdeologyRoleSelectionOption(null, clearCurrentRole: true));
            }

            return options;
        }

        private static string GetDisplayLabel(IdeologyRoleSelectionOption option)
        {
            if (option.ClearCurrentRole)
            {
                return "CheatMenu.Ideology.SetRole.Window.NoneOption".Translate().ToString();
            }

            return option.Role.LabelCap.ToString();
        }
    }
}
