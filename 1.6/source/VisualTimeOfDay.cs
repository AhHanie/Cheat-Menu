namespace Cheat_Menu
{
    public enum VisualTimeOfDay
    {
        Normal,
        Dawn,
        Noon,
        Dusk,
        Midnight
    }

    public static class VisualTimeOfDayUtility
    {
        // Dawn and Dusk share the dusk sky-color threshold but use opposite day fractions
        // so GenCelestial's shadow/water math places the sun on opposite sides of the map.
        public static bool TryGetRenderInputs(VisualTimeOfDay preset, out float dayFraction, out float glow)
        {
            switch (preset)
            {
                case VisualTimeOfDay.Dawn:
                    dayFraction = 0.25f;
                    glow = 0.6f;
                    return true;
                case VisualTimeOfDay.Noon:
                    dayFraction = 0.5f;
                    glow = 1f;
                    return true;
                case VisualTimeOfDay.Dusk:
                    dayFraction = 0.75f;
                    glow = 0.6f;
                    return true;
                case VisualTimeOfDay.Midnight:
                    dayFraction = 0f;
                    glow = 0f;
                    return true;
                default:
                    dayFraction = 0f;
                    glow = 0f;
                    return false;
            }
        }
    }
}
