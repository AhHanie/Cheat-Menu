using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Fluent helper for composing a cheat flow from one or more step types.
    /// AddAction/AddTool/AddWindow can be chained to create composite behavior.
    /// </summary>
    public sealed class CheatBuilder
    {
        private readonly string id;
        private readonly string labelKey;
        private readonly string descriptionKey;
        private readonly List<ICheatStep> steps = new List<ICheatStep>();

        private string categoryKey;
        private bool requiresMap;
        private CheatAllowedGameStates allowedGameStates = CheatAllowedGameStates.Playing;
        private bool requiresRoyalty;
        private bool requiresIdeology;
        private bool requiresBiotech;
        private bool requiresAnomaly;
        private bool requiresOdyssey;
        private Func<bool> visibilityGetter;

        private CheatBuilder(string id, string labelKey, string descriptionKey)
        {
            this.id = id;
            this.labelKey = labelKey;
            this.descriptionKey = descriptionKey;
        }

        public static CheatBuilder Create(string id, string labelKey, string descriptionKey)
        {
            return new CheatBuilder(id, labelKey, descriptionKey);
        }

        public CheatBuilder InCategory(string categoryKey)
        {
            this.categoryKey = categoryKey;
            return this;
        }

        public CheatBuilder RequireMap(bool value = true)
        {
            requiresMap = value;
            return this;
        }

        public CheatBuilder AllowedIn(CheatAllowedGameStates states)
        {
            allowedGameStates = states;
            return this;
        }

        public CheatBuilder RequireRoyalty(bool value = true)
        {
            requiresRoyalty = value;
            return this;
        }

        public CheatBuilder RequireIdeology(bool value = true)
        {
            requiresIdeology = value;
            return this;
        }

        public CheatBuilder RequireBiotech(bool value = true)
        {
            requiresBiotech = value;
            return this;
        }

        public CheatBuilder RequireAnomaly(bool value = true)
        {
            requiresAnomaly = value;
            return this;
        }

        public CheatBuilder RequireOdyssey(bool value = true)
        {
            requiresOdyssey = value;
            return this;
        }

        public CheatBuilder VisibleWhen(Func<bool> visibilityGetter)
        {
            this.visibilityGetter = visibilityGetter;
            return this;
        }

        public CheatBuilder AddAction(Action<CheatExecutionContext> action)
        {
            steps.Add(new CheatActionStep(action));
            return this;
        }

        public CheatBuilder AddTool(
            Action<CheatExecutionContext, LocalTargetInfo> onTargetSelected,
            Func<CheatExecutionContext, TargetingParameters> targetingParametersFactory = null,
            string startMessageKey = null,
            bool repeatTargeting = false)
        {
            steps.Add(new CheatToolStep(onTargetSelected, targetingParametersFactory, startMessageKey, repeatTargeting));
            return this;
        }

        public CheatBuilder AddWindow(Action<CheatExecutionContext, Action> openWindow)
        {
            steps.Add(new CheatWindowStep(openWindow));
            return this;
        }

        public CheatDefinition Build()
        {
            return new CheatDefinition(
                id,
                labelKey,
                descriptionKey,
                steps,
                categoryKey,
                requiresMap,
                allowedGameStates,
                requiresRoyalty,
                requiresIdeology,
                requiresBiotech,
                requiresAnomaly,
                requiresOdyssey,
                visibilityGetter);
        }
    }
}
