using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public class CompCheatStatOffsets : ThingComp
    {
        private Dictionary<StatDef, float> statOffsets = new Dictionary<StatDef, float>();

        public IReadOnlyDictionary<StatDef, float> StatOffsets => statOffsets;

        public bool TryGetOffset(StatDef statDef, out float value)
        {
            return statOffsets.TryGetValue(statDef, out value);
        }

        public void SetOffset(StatDef statDef, float value)
        {
            statOffsets[statDef] = value;
        }

        public bool RemoveOffset(StatDef statDef)
        {
            return statOffsets.Remove(statDef);
        }

        public void ClearOffsets()
        {
            statOffsets.Clear();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref statOffsets, "statOffsets", LookMode.Def, LookMode.Value);
            if (statOffsets == null)
            {
                statOffsets = new Dictionary<StatDef, float>();
            }
        }

        public override float GetStatOffset(StatDef stat)
        {
            float offset;
            if (!statOffsets.TryGetValue(stat, out offset))
            {
                return 0f;
            }

            return offset;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
        {
            if (!statOffsets.TryGetValue(stat, out float offset) || offset == 0f)
            {
                return;
            }

            sb.AppendLine(
                $"{whitespace}{"CheatMenu.Cheats.EditStatOffsets.Explanation".Translate()}: {offset.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset)}");
        }
    }
}
