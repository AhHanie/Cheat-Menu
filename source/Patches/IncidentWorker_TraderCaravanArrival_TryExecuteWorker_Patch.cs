using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns")]
    public static class IncidentWorker_TraderCaravanArrival_SpawnPawns_Patch
    {
        private const int TargetSilverAmount = 50000;

        public static void Postfix(IncidentWorker_NeutralGroup __instance, List<Pawn> __result)
        {
            if (!(__instance is IncidentWorker_TraderCaravanArrival) || !IsRichMerchantsEnabled() || __result == null || __result.Count == 0)
            {
                return;
            }

            AddSilverToCaravan(__result, TargetSilverAmount);
        }

        private static bool IsRichMerchantsEnabled()
        {
            return Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.RichMerchantsKey);
        }

        private static void AddSilverToCaravan(List<Pawn> pawns, int silverToAdd)
        {
            int remaining = silverToAdd;
            float silverMass = ThingDefOf.Silver.GetStatValueAbstract(StatDefOf.Mass);

            for (int i = 0; i < pawns.Count && remaining > 0; i++)
            {
                remaining -= AddSilverToPawn(pawns[i], remaining, silverMass);
            }
        }

        private static int AddSilverToPawn(Pawn pawn, int requestedAmount, float silverMass)
        {
            if (pawn.inventory == null)
            {
                return 0;
            }

            float freeMass = MassUtility.Capacity(pawn) - MassUtility.GearAndInventoryMass(pawn);
            int maxByMass = (int)(freeMass / silverMass);
            if (maxByMass <= 0)
            {
                return 0;
            }

            int remainingForPawn = requestedAmount > maxByMass ? maxByMass : requestedAmount;
            int added = 0;
            int stackLimit = ThingDefOf.Silver.stackLimit;

            while (remainingForPawn > 0)
            {
                int count = remainingForPawn > stackLimit ? stackLimit : remainingForPawn;
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = count;
                if (!pawn.inventory.innerContainer.TryAdd(silver))
                {
                    silver.Destroy();
                    break;
                }

                remainingForPawn -= count;
                added += count;
            }

            return added;
        }
    }
}
