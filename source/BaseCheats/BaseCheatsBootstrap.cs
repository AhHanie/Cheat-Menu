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
            SpawnThingCheat.Register();
            PawnResurrectionCheat.Register();
            PawnHealRandomInjuryCheat.Register();
            PawnGiveAbilityCheat.Register();
            PawnAddGeneCheat.Register();
            PawnGiveTraitCheat.Register();
            PawnStartInspirationCheat.Register();
            PawnSetSkillCheat.Register();
            PawnSetPassionCheat.Register();
            PawnRecruitCheat.Register();
            PawnTameAnimalCheat.Register();
            GeneralCheats.Register();
        }
    }
}
