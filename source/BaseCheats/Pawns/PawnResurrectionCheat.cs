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
            int affected = 0;
            foreach (Thing item in target.Cell.GetThingList(Find.CurrentMap).ToList())
            {
                if (item is Corpse corpse)
                {
                    if (ResurrectionUtility.TryResurrect(corpse.InnerPawn))
                    {
                        affected++;
                    }
                }
            }

            if (affected == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnResurrection.Message.NoCorpse".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnResurrection.Message.Result".Translate(affected),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
