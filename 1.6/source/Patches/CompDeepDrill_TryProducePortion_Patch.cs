using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CompDeepDrill), "TryProducePortion")]
    public static class CompDeepDrill_TryProducePortion_Patch
    {
        public static bool DeepDrillingFlag = false;
        public static void Prefix(ref bool __state)
        {
            __state = IsInfiniteDeepDrillingEnabled();
            if (!__state)
            {
                return;
            }

            DeepDrillingFlag = true;
        }

        public static void Postfix(bool __state)
        {
            if (!__state)
            {
                return;
            }

            DeepDrillingFlag = false;
        }

        private static bool IsInfiniteDeepDrillingEnabled()
        {
            return Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.InfiniteDeepDrillingKey);
        }
    }
}
