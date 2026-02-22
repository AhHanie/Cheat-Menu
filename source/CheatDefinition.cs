using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    [Flags]
    public enum CheatAllowedGameStates
    {
        Playing = 1,
        Entry = 2,
        WorldRenderedNow = 4,
        IsCurrentlyOnMap = 8,
        HasGameCondition = 16,

        PlayingOnMap = Playing | IsCurrentlyOnMap
    }

    [Flags]
    public enum CheatKinds
    {
        None = 0,
        Action = 1,
        Tool = 2,
        Window = 4
    }

    public sealed class CheatDefinition
    {
        private readonly List<ICheatStep> steps;
        private readonly bool requiresMap;
        private readonly CheatAllowedGameStates allowedGameStates;
        private readonly bool requiresRoyalty;
        private readonly bool requiresIdeology;
        private readonly bool requiresBiotech;
        private readonly bool requiresAnomaly;
        private readonly bool requiresOdyssey;
        private readonly Func<bool> visibilityGetter;

        public CheatDefinition(
            string id,
            string labelKey,
            string descriptionKey,
            IEnumerable<ICheatStep> steps,
            string categoryKey = null,
            bool requiresMap = false,
            CheatAllowedGameStates allowedGameStates = CheatAllowedGameStates.Playing,
            bool requiresRoyalty = false,
            bool requiresIdeology = false,
            bool requiresBiotech = false,
            bool requiresAnomaly = false,
            bool requiresOdyssey = false,
            Func<bool> visibilityGetter = null)
        {
            if (id.NullOrEmpty())
            {
                throw new ArgumentException("A non-empty cheat id is required.", nameof(id));
            }

            if (steps == null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            this.steps = steps.Where(step => step != null).ToList();
            if (this.steps.Count == 0)
            {
                throw new ArgumentException("At least one cheat step is required.", nameof(steps));
            }

            Id = id;
            LabelKey = labelKey;
            DescriptionKey = descriptionKey;
            CategoryKey = categoryKey;
            this.requiresMap = requiresMap;
            this.allowedGameStates = allowedGameStates;
            this.requiresRoyalty = requiresRoyalty;
            this.requiresIdeology = requiresIdeology;
            this.requiresBiotech = requiresBiotech;
            this.requiresAnomaly = requiresAnomaly;
            this.requiresOdyssey = requiresOdyssey;
            this.visibilityGetter = visibilityGetter;
        }

        public string Id { get; }

        public string LabelKey { get; }

        public string DescriptionKey { get; }

        public string CategoryKey { get; }

        public IReadOnlyList<ICheatStep> Steps => steps;

        public bool IsComposite => steps.Count > 1;

        public bool RequiresMap => requiresMap || steps.Any(step => step.StepType == CheatStepType.Tool);

        public CheatAllowedGameStates AllowedGameStates => allowedGameStates;

        public CheatKinds Kinds
        {
            get
            {
                CheatKinds result = CheatKinds.None;
                for (int i = 0; i < steps.Count; i++)
                {
                    switch (steps[i].StepType)
                    {
                        case CheatStepType.Action:
                            result |= CheatKinds.Action;
                            break;
                        case CheatStepType.Tool:
                            result |= CheatKinds.Tool;
                            break;
                        case CheatStepType.Window:
                            result |= CheatKinds.Window;
                            break;
                    }
                }

                return result;
            }
        }

        public string GetLabel()
        {
            return LabelKey.NullOrEmpty() ? Id : LabelKey.Translate().ToString();
        }

        public string GetDescription()
        {
            if (DescriptionKey.NullOrEmpty())
            {
                return "CheatMenu.Window.NoDescription".Translate();
            }

            return DescriptionKey.Translate().ToString();
        }

        public string GetCategoryOrDefault()
        {
            return CategoryKey.NullOrEmpty()
                ? "CheatMenu.Category.Uncategorized".Translate().ToString()
                : CategoryKey.Translate().ToString();
        }

        public bool IsVisibleNow()
        {
            bool entryAllowed = (allowedGameStates & CheatAllowedGameStates.Entry) == 0
                || Current.ProgramState == ProgramState.Entry;

            bool playingAllowed = (allowedGameStates & CheatAllowedGameStates.Playing) == 0
                || Current.ProgramState == ProgramState.Playing;

            bool worldRenderedAllowed = (allowedGameStates & CheatAllowedGameStates.WorldRenderedNow) == 0
                || WorldRendererUtility.WorldSelected;

            bool mapAllowed = (allowedGameStates & CheatAllowedGameStates.IsCurrentlyOnMap) == 0
                || (!WorldRendererUtility.WorldSelected && Find.CurrentMap != null);

            bool gameConditionAllowed = (allowedGameStates & CheatAllowedGameStates.HasGameCondition) == 0
                || (!WorldRendererUtility.WorldSelected
                    && Find.CurrentMap != null
                    && Find.CurrentMap.gameConditionManager.ActiveConditions.Count > 0);

            bool dlcAllowed = (!requiresRoyalty || ModsConfig.RoyaltyActive)
                && (!requiresIdeology || ModsConfig.IdeologyActive)
                && (!requiresBiotech || ModsConfig.BiotechActive)
                && (!requiresAnomaly || ModsConfig.AnomalyActive)
                && (!requiresOdyssey || ModsConfig.OdysseyActive);

            bool customVisible = true;
            if (visibilityGetter != null)
            {
                try
                {
                    customVisible = visibilityGetter();
                }
                catch (Exception ex)
                {
                    UserLogger.Exception(ex, "Cheat visibility getter failed for '" + Id + "'");
                    customVisible = false;
                }
            }

            return entryAllowed
                && playingAllowed
                && worldRenderedAllowed
                && mapAllowed
                && gameConditionAllowed
                && dlcAllowed
                && customVisible;
        }
    }
}
