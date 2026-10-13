using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
    public static class CompLaunchable_TryLaunch_Patch
    {
        public static void Postfix(CompLaunchable __instance)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableShuttleCooldownKey))
            {
                return;
            }

            if (__instance.parent.GetComp<CompShuttle>() == null)
            {
                return;
            }

            __instance.lastLaunchTick = -1;
        }
    }
}
