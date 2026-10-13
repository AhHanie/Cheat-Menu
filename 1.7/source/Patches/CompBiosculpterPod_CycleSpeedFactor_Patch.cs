using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CompBiosculpterPod), "get_CycleSpeedFactor")]
    public static class CompBiosculpterPod_CycleSpeedFactor_Patch
    {
        private const float FastBiosculptingSpeedFactor = 200f;

        public static bool Prefix(ref float __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.FastBiosculptingKey))
            {
                return true;
            }

            __result = FastBiosculptingSpeedFactor;
            return false;
        }
    }
}
