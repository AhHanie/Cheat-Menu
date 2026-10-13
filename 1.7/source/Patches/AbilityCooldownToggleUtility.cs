using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class AbilityCooldownToggleUtility
    {
        public static bool ShouldDisableCooldown(Ability ability)
        {
            if (!Current.Game.GetComponent<CheatMenuGameComponent>().IsEnabled(ToggleCheatsGeneral.DisableAbilityCooldownKey))
            {
                return false;
            }

            return ability.pawn != null && ability.pawn.IsColonist;
        }

        public static void RestoreCharges(Ability ability)
        {
            if (!ShouldDisableCooldown(ability) || !ability.UsesCharges)
            {
                return;
            }

            ability.RemainingCharges = ability.maxCharges;
        }

        public static void RestoreChargesForColonists()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
            {
                if (pawn.abilities == null)
                {
                    continue;
                }

                foreach (Ability ability in pawn.abilities.AllAbilitiesForReading)
                {
                    RestoreCharges(ability);
                }
            }
        }
    }
}
