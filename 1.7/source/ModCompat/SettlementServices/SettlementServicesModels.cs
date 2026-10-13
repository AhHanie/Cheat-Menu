using Verse;

namespace Cheat_Menu
{
    public sealed class SettlementServicesJobOption
    {
        public SettlementServicesJobOption(
            int jobId,
            string serviceDefName,
            string serviceLabel,
            int settlementWorldObjectId,
            string settlementLabel,
            string targetSummary,
            int quantity,
            int expectedCompletionTick)
        {
            JobId = jobId;
            ServiceDefName = serviceDefName;
            ServiceLabel = serviceLabel;
            SettlementWorldObjectId = settlementWorldObjectId;
            SettlementLabel = settlementLabel;
            TargetSummary = targetSummary;
            Quantity = quantity;
            ExpectedCompletionTick = expectedCompletionTick;
        }

        public int JobId { get; }

        public string ServiceDefName { get; }

        public string ServiceLabel { get; }

        public int SettlementWorldObjectId { get; }

        public string SettlementLabel { get; }

        public string TargetSummary { get; }

        public int Quantity { get; }

        public int ExpectedCompletionTick { get; }
    }

    public sealed class SettlementServicesSpecialtyOption
    {
        public SettlementServicesSpecialtyOption(Def specialtyDef, string defName, string label, bool disabled)
        {
            SpecialtyDef = specialtyDef;
            DefName = defName;
            Label = label;
            Disabled = disabled;
        }

        public Def SpecialtyDef { get; }

        public string DefName { get; }

        public string Label { get; }

        public bool Disabled { get; }
    }

    public sealed class SettlementServicesEventOption
    {
        public SettlementServicesEventOption(Def eventDef, string defName, string label, string triggerPhaseText)
        {
            EventDef = eventDef;
            DefName = defName;
            Label = label;
            TriggerPhaseText = triggerPhaseText;
        }

        public Def EventDef { get; }

        public string DefName { get; }

        public string Label { get; }

        public string TriggerPhaseText { get; }
    }
}
