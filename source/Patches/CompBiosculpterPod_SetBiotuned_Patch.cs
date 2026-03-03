using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CompBiosculpterPod), "SetBiotuned")]
    public static class CompBiosculpterPod_SetBiotuned_Patch
    {
        public static bool Prefix()
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableBiosculpterBiotuningKey))
            {
                return true;
            }

            return false;
        }
    }
}
