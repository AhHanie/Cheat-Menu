using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(StorytellerComp), "IncidentChanceFinal")]
    public static class StorytellerComp_IncidentChanceFinal_Patch
    {
        public static bool Prefix(IncidentDef def, ref float __result)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableSolarFlaresKey))
            {
                return true;
            }

            if (def != IncidentDefOf.SolarFlare)
            {
                return true;
            }

            __result = 0f;
            return false;
        }
    }
}
