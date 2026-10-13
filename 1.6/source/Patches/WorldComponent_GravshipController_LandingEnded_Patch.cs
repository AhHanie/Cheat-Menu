using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld.Planet;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static void Prefix(Gravship ___gravship, ref bool __state)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableGravshipLandingOutcomesKey))
            {
                return;
            }

            __state = ___gravship.Engine.launchInfo.doNegativeOutcome;
            ___gravship.Engine.launchInfo.doNegativeOutcome = false;
        }
    }
}
