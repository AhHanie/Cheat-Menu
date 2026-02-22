using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnResurrectionCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnResurrection",
                "CheatMenu.Cheat.PawnResurrection.Label",
                "CheatMenu.Cheat.PawnResurrection.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        ResurrectAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat"));
        }

        private static TargetingParameters CreateCellTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetItems = false
            };
        }

        private static void ResurrectAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            if (map == null || !cell.IsValid || !cell.InBounds(map))
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Thing> thingsAtCell = cell.GetThingList(map).ToList();
            int corpsesFound = 0;
            int pawnsResurrected = 0;

            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                Corpse corpse = thingsAtCell[i] as Corpse;
                if (corpse == null || corpse.InnerPawn == null)
                {
                    continue;
                }

                corpsesFound++;
                if (ResurrectionUtility.TryResurrect(corpse.InnerPawn))
                {
                    pawnsResurrected++;
                }
            }

            if (corpsesFound == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnResurrection.Message.NoCorpse".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnResurrection.Message.Result".Translate(pawnsResurrected, corpsesFound),
                pawnsResurrected > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
