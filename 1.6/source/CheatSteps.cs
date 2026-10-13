using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public enum CheatStepType
    {
        Action,
        Tool,
        Window
    }

    public interface ICheatStep
    {
        CheatStepType StepType { get; }

        // Implementations must call continueFlow when their work is complete.
        void Execute(CheatDefinition cheat, CheatExecutionContext context, Action continueFlow);
    }

    public sealed class CheatActionStep : ICheatStep
    {
        private readonly Action<CheatExecutionContext> action;

        public CheatActionStep(Action<CheatExecutionContext> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public CheatStepType StepType => CheatStepType.Action;

        public void Execute(CheatDefinition cheat, CheatExecutionContext context, Action continueFlow)
        {
            action(context);
            continueFlow?.Invoke();
        }
    }

    public sealed class CheatToolStep : ICheatStep
    {
        private readonly Action<CheatExecutionContext, LocalTargetInfo> onTargetSelected;
        private readonly Func<CheatExecutionContext, TargetingParameters> targetingParametersFactory;
        private readonly string startMessageKey;
        private readonly bool repeatTargeting;

        public CheatToolStep(
            Action<CheatExecutionContext, LocalTargetInfo> onTargetSelected,
            Func<CheatExecutionContext, TargetingParameters> targetingParametersFactory = null,
            string startMessageKey = null,
            bool repeatTargeting = false)
        {
            this.onTargetSelected = onTargetSelected ?? throw new ArgumentNullException(nameof(onTargetSelected));
            this.targetingParametersFactory = targetingParametersFactory;
            this.startMessageKey = startMessageKey ?? "CheatMenu.Message.SelectTarget";
            this.repeatTargeting = repeatTargeting;
        }

        public CheatStepType StepType => CheatStepType.Tool;

        public void Execute(CheatDefinition cheat, CheatExecutionContext context, Action continueFlow)
        {
            if (Find.CurrentMap == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.Message.RequiresMap".Translate(cheat.GetLabel()),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            if (!startMessageKey.NullOrEmpty())
            {
                CheatMessageService.Message(startMessageKey.Translate(cheat.GetLabel()), MessageTypeDefOf.NeutralEvent, false);
            }

            TargetingParameters targetingParameters = targetingParametersFactory != null
                ? targetingParametersFactory(context)
                : DefaultTargetingParameters();

            Action startTargeting = null;
            startTargeting = delegate
            {
                Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
                {
                    context.LastTarget = target;
                    onTargetSelected(context, target);

                    if (repeatTargeting && Find.CurrentMap != null)
                    {
                        startTargeting();
                        return;
                    }

                    continueFlow?.Invoke();
                });
            };

            startTargeting();
        }

        private static TargetingParameters DefaultTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = true,
                canTargetPawns = true,
                canTargetItems = true
            };
        }
    }

    public sealed class CheatWindowStep : ICheatStep
    {
        private readonly Action<CheatExecutionContext, Action> openWindow;

        public CheatWindowStep(Action<CheatExecutionContext, Action> openWindow)
        {
            this.openWindow = openWindow ?? throw new ArgumentNullException(nameof(openWindow));
        }

        public CheatStepType StepType => CheatStepType.Window;

        public void Execute(CheatDefinition cheat, CheatExecutionContext context, Action continueFlow)
        {
            openWindow(context, continueFlow);
        }
    }
}
