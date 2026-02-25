namespace Cheat_Menu
{
    public static class MapCheats
    {
        public static void Register()
        {
            MapFogCheats.Register();
            MapAddGameConditionCheat.Register();
            MapRemoveGameConditionCheat.Register();
            MapSetTerrainRectCheat.Register();
        }
    }
}
