using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CaravanShuttleUtility), nameof(CaravanShuttleUtility.LaunchShuttle))]
    public static class CaravanShuttleUtility_LaunchShuttle_Patch
    {
        public static void Prefix(Caravan caravan, ref Building_PassengerShuttle __state)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableShuttleCooldownKey))
            {
                return;
            }

            __state = caravan.Shuttle;
        }

        public static void Postfix(Caravan caravan, Building_PassengerShuttle __state)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableShuttleCooldownKey))
            {
                return;
            }

            if (__state == null)
            {
                return;
            }

            __state.LaunchableComp.lastLaunchTick = -1;
        }
    }
}
