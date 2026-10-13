using HarmonyLib;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    [HarmonyPatch(typeof(TradeShip), nameof(TradeShip.GenerateThings))]
    public static class TradeShip_GenerateThings_Patch
    {
        private const int SilverToAdd = 50000;

        public static void Postfix(TradeShip __instance)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.RichMerchantsKey))
            {
                return;
            }

            AddSilver(__instance, SilverToAdd);
        }

        private static void AddSilver(TradeShip tradeShip, int totalAmount)
        {
            ThingOwner heldThings = tradeShip.GetDirectlyHeldThings();
            int stackLimit = ThingDefOf.Silver.stackLimit;
            int remaining = totalAmount;

            while (remaining > 0)
            {
                int count = remaining > stackLimit ? stackLimit : remaining;
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = count;
                heldThings.TryAdd(silver);
                remaining -= count;
            }
        }
    }
}
