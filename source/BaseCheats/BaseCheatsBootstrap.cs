namespace Cheat_Menu
{
    public static class BaseCheatsBootstrap
    {
        private static bool registered;

        public static void RegisterAll()
        {
            if (registered)
            {
                return;
            }

            registered = true;
            IncidentCheat.Register();
            SpawningCheats.Register();
            PawnResurrectionCheat.Register();
            PawnHealRandomInjuryCheat.Register();
            PawnGiveAbilityCheat.Register();
            PawnAddGeneCheat.Register();
            PawnSetXenotypeCheat.Register();
            PawnGiveTraitCheat.Register();
            PawnStartInspirationCheat.Register();
            PawnSetSkillCheat.Register();
            PawnSetPassionCheat.Register();
            PawnSetBackstoryCheat.Register();
            PawnRecruitCheat.Register();
            PawnTameAnimalCheat.Register();
            PawnDamageCheat.Register();
            GeneralCheats.Register();
        }
    }
}
