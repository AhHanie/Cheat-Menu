using RimWorld.Planet;
using Verse;

namespace Cheat_Menu
{
    // Intentionally has no [HarmonyPatch] attribute: Mod.Init()'s PatchAll() must not install this
    // automatically. VisualTimeOfDayPatchController patches/unpatches it manually, only while at
    // least one loaded map has a non-Normal preset.
    public static class SkyManager_SkyManagerUpdate_Patch
    {
        public static void Postfix(Map ___map)
        {
            if (Current.Game == null || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Map map = ___map;
            if (map == null || map != Find.CurrentMap || WorldRendererUtility.WorldSelected)
            {
                return;
            }

            CheatMenuMapComponent mapComponent = map.GetComponent<CheatMenuMapComponent>();
            VisualTimeOfDay preset = mapComponent != null ? mapComponent.VisualTimeOfDayPreset : VisualTimeOfDay.Normal;
            if (preset == VisualTimeOfDay.Normal)
            {
                return;
            }

            if (map.Biome != null && map.Biome.disableSkyLighting)
            {
                return;
            }

            if (!VisualTimeOfDayUtility.TryGetRenderInputs(preset, out float dayFraction, out float glow))
            {
                return;
            }

            VisualTimeOfDayRenderer.Apply(map, dayFraction, glow);
        }
    }
}
