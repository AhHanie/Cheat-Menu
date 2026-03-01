namespace Cheat_Menu
{
    public static class ToggleCheatBoostrap
    {
        private static bool registered;

        public static void RegisterAll()
        {
            if (registered)
            {
                return;
            }

            registered = true;
            ToggleCheatsNeeds.Register();
            ToggleCheatsGeneral.Register();
        }
    }
}

