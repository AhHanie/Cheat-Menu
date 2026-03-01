using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class GenRecipe_PostProcessProduct_Patch
    {
        public static bool PostProcessProductFlag = false;
        public static void Prefix(ref bool __state)
        {
            __state = IsAlwaysCraftLegendariesEnabled();
            if (!__state)
            {
                return;
            }

            PostProcessProductFlag = true;
        }

        public static void Postfix(bool __state)
        {
            if (!__state)
            {
                return;
            }

            PostProcessProductFlag = false;
        }

        private static bool IsAlwaysCraftLegendariesEnabled()
        {
            return Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.AlwaysCraftLegendariesKey);
        }
    }
}

