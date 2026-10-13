using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [DefOf]
    public static class VREAndroidsDefOf
    {
        [MayRequire("vanillaracesexpanded.android")]
        public static HediffDef VREA_NeutroLoss;

        static VREAndroidsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VREAndroidsDefOf));
        }
    }
}
