using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [DefOf]
    public static class CheatMenuThingDefOf
    {
        [MayRequireOdyssey]
        public static ThingDef GravEngine;

        [MayRequireOdyssey]
        public static ThingDef PilotConsole;

        [MayRequireOdyssey]
        public static ThingDef ChemfuelTank;

        [MayRequireOdyssey]
        public static ThingDef SmallThruster;

        [MayRequireOdyssey]
        public static ThingDef GravshipHull;

        [MayRequireOdyssey]
        public static TerrainDef Substructure;

        static CheatMenuThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CheatMenuThingDefOf));
        }
    }
}
