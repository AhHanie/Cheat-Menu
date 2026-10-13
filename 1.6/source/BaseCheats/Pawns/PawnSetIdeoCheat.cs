using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnSetIdeoCheat
    {
        private const string SelectedIdeoContextKey = "BaseCheats.Pawns.SetIdeo.Selected";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnSetIdeo",
                "CheatMenu.Cheat.PawnSetIdeo.Label",
                "CheatMenu.Cheat.PawnSetIdeo.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddWindow(OpenIdeoSelectionWindow)
                    .AddTool(
                        ApplyIdeoAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.PawnSetIdeo.Message.SelectCell",
                        repeatTargeting: true));
        }

        private static void OpenIdeoSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new PawnIdeoSelectionWindow(delegate (Ideo selected)
            {
                context.Set(SelectedIdeoContextKey, selected);
                continueFlow?.Invoke();
            }));
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

        private static void ApplyIdeoAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedIdeoContextKey, out Ideo selectedIdeo))
            {
                CheatMessageService.Message("CheatMenu.PawnSetIdeo.Message.NoIdeoSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<Pawn> pawnsAtCell = target.Cell.GetThingList(Find.CurrentMap).OfType<Pawn>().ToList();
            if (pawnsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnSetIdeo.Message.NoPawn".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            int updatedCount = 0;
            for (int i = 0; i < pawnsAtCell.Count; i++)
            {
                Pawn pawn = pawnsAtCell[i];
                if (!pawn.RaceProps.Humanlike)
                {
                    continue;
                }

                pawn.ideo.SetIdeo(selectedIdeo);
                DebugActionsUtility.DustPuffFrom(pawn);
                updatedCount++;
            }

            if (updatedCount == 0)
            {
                CheatMessageService.Message("CheatMenu.PawnSetIdeo.Message.NoHumanlikePawns".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.PawnSetIdeo.Message.Result".Translate(updatedCount, selectedIdeo.name),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
