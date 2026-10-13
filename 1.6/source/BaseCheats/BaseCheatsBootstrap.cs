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
            IncidentCheats.Register();
            QuestCheats.Register();
            SpawningCheats.Register();
            PawnCheats.Register();
            MapCheats.Register();
            GeneralCheats.Register();
            CheatsCategoryCheats.Register();
            IdeologyCheats.Register();
        }
    }
}
