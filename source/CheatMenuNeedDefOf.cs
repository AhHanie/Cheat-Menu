using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [DefOf]
    public static class CheatMenuNeedDefOf
    {
        public static NeedDef Food;
        public static NeedDef Rest;
        public static NeedDef Indoors;

        [MayRequireBiotech]
        public static NeedDef MechEnergy;

        public static NeedDef Joy;
        public static NeedDef Beauty;
        public static NeedDef Comfort;
        public static NeedDef Outdoors;
        public static NeedDef Mood;

        static CheatMenuNeedDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CheatMenuNeedDefOf));
        }
    }
}

