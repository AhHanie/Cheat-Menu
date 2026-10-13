using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterGrowPlantToMaturity()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralGrowPlantToMaturity",
                "CheatMenu.General.GrowPlantToMaturity.Label",
                "CheatMenu.General.GrowPlantToMaturity.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        GrowPlantToMaturityAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void RegisterGrowPlantOneDay()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralGrowPlantOneDay",
                "CheatMenu.General.GrowPlantOneDay.Label",
                "CheatMenu.General.GrowPlantOneDay.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        GrowPlantOneDayAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void GrowPlantToMaturityAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            Plant plant = cell.GetPlant(map);
            if (plant == null || plant.def.plant == null)
            {
                CheatMessageService.Message("CheatMenu.General.GrowPlant.Message.NoPlant".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int growthRemaining = (int)((1f - plant.Growth) * plant.def.plant.growDays);
            plant.Age += growthRemaining;
            plant.Growth = 1f;

            map.mapDrawer.SectionAt(cell).RegenerateAllLayers();
        }

        private static void GrowPlantOneDayAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            Plant plant = cell.GetPlant(map);
            if (plant == null || plant.def.plant == null)
            {
                CheatMessageService.Message("CheatMenu.General.GrowPlant.Message.NoPlant".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int growthRemaining = (int)((1f - plant.Growth) * plant.def.plant.growDays);
            if (growthRemaining >= 60000)
            {
                plant.Age += 60000;
            }
            else if (growthRemaining > 0)
            {
                plant.Age += growthRemaining;
            }

            plant.Growth += 1f / plant.def.plant.growDays;
            if (plant.Growth > 1f)
            {
                plant.Growth = 1f;
            }

            map.mapDrawer.SectionAt(cell).RegenerateAllLayers();
        }
    }
}
