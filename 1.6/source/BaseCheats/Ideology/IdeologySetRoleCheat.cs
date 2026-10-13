using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class IdeologyCheats
    {
        private static void RegisterSetIdeoRole()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.IdeologySetIdeoRole",
                "CheatMenu.Ideology.SetRole.Label",
                "CheatMenu.Ideology.SetRole.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Ideology")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddTool(
                        OpenRoleSelectionForTargetPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.Ideology.SetRole.Message.SelectPawn"));
        }

        private static void OpenRoleSelectionForTargetPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.Ideology.SetRole.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (pawn.Ideo == null)
            {
                CheatMessageService.Message("CheatMenu.Ideology.SetRole.Message.NoIdeo".Translate(pawn.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<IdeologyRoleSelectionOption> options = IdeologyRoleSelectionWindow.BuildRoleOptions(pawn);
            if (options.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Ideology.SetRole.Message.NoRolesAvailable".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new IdeologyRoleSelectionWindow(pawn, options, selectedOption =>
            {
                ApplyRoleSelection(pawn, selectedOption);
            }));
        }

        private static void ApplyRoleSelection(Pawn pawn, IdeologyRoleSelectionOption selectedOption)
        {
            if (selectedOption.ClearCurrentRole)
            {
                Precept_Role currentRole = pawn.Ideo.GetRole(pawn);
                if (currentRole == null)
                {
                    CheatMessageService.Message("CheatMenu.Ideology.SetRole.Message.NoRolesAvailable".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                currentRole.Assign(null, addThoughts: true);
                DebugActionsUtility.DustPuffFrom(pawn);

                CheatMessageService.Message(
                    "CheatMenu.Ideology.SetRole.Message.ResultCleared".Translate(pawn.LabelShortCap),
                    MessageTypeDefOf.PositiveEvent,
                    false);

                return;
            }

            selectedOption.Role.Assign(pawn, addThoughts: true);
            DebugActionsUtility.DustPuffFrom(pawn);

            CheatMessageService.Message(
                "CheatMenu.Ideology.SetRole.Message.ResultAssigned".Translate(pawn.LabelShortCap, selectedOption.Role.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
