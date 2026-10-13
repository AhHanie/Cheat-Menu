using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public static class CheatStatOffsetsCompInjector
    {
        private static bool injected;

        public static void Inject()
        {
            if (injected)
            {
                return;
            }

            injected = true;

            List<ThingDef> allThingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < allThingDefs.Count; i++)
            {
                ThingDef thingDef = allThingDefs[i];
                if (thingDef.thingClass == null || !typeof(ThingWithComps).IsAssignableFrom(thingDef.thingClass))
                {
                    continue;
                }

                if (thingDef.comps == null)
                {
                    thingDef.comps = new List<CompProperties>(1);
                }

                bool hasComp = false;
                for (int compIndex = 0; compIndex < thingDef.comps.Count; compIndex++)
                {
                    CompProperties compProperties = thingDef.comps[compIndex];
                    if (compProperties.compClass == typeof(CompCheatStatOffsets))
                    {
                        hasComp = true;
                        break;
                    }
                }

                if (!hasComp)
                {
                    thingDef.comps.Add(new CompProperties_CheatStatOffsets());
                }
            }
        }
    }
}
