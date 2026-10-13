using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(PassengerShuttleIncoming), "Impact")]
    public static class PassengerShuttleIncoming_Impact_Patch
    {
        public static void Prefix(PassengerShuttleIncoming __instance, ref Building_PassengerShuttle __state)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableShuttleCooldownKey))
            {
                return;
            }

            __state = __instance.Shuttle;
        }

        public static void Postfix(Building_PassengerShuttle __state)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableShuttleCooldownKey))
            {
                return;
            }

            __state.LaunchableComp.lastLaunchTick = -1;
        }
    }
}
