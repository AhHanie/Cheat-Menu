using System;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnGivePsylinkCheat
    {
        private const string SelectedLevelContextKey = "BaseCheats.Pawns.GivePsylink.SelectedLevel";

        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnGivePsylink",
                "CheatMenu.Cheat.PawnGivePsylink.Label",
                "CheatMenu.Cheat.PawnGivePsylink.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireRoyalty()
                    .AddWindow(OpenLevelSelectionWindow)
                    .AddTool(
                        ApplyPsylinkToPawn,
                        CreatePawnTargetingParameters,
                        "CheatMenu.PawnGivePsylink.Message.SelectPawn",
                        repeatTargeting: true));
        }

        private static void OpenLevelSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            int maxLevel = GetMaxPsylinkLevel();

            Find.WindowStack.Add(new AmountSelectionWindow(
                "CheatMenu.PawnGivePsylink.Window.Title",
                "CheatMenu.PawnGivePsylink.Window.Description",
                maxLevel,
                1,
                maxLevel,
                delegate (int selectedLevel)
                {
                    context.Set(SelectedLevelContextKey, selectedLevel);
                    continueFlow?.Invoke();
                }));
        }

        private static TargetingParameters CreatePawnTargetingParameters(CheatExecutionContext context)
        {
            Find.MainTabsRoot?.EscapeCurrentTab();

            return new TargetingParameters
            {
                canTargetLocations = false,
                canTargetBuildings = false,
                canTargetPawns = true,
                canTargetItems = false
            };
        }

        private static void ApplyPsylinkToPawn(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedLevelContextKey, out int selectedLevel))
            {
                CheatMessageService.Message("CheatMenu.PawnGivePsylink.Message.NoLevelSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Pawn pawn = target.HasThing ? target.Thing as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                CheatMessageService.Message("CheatMenu.PawnGivePsylink.Message.InvalidPawnTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Hediff_Level hediffLevel = pawn.GetMainPsylinkSource();
            if (hediffLevel == null)
            {
                BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
                if (brain == null)
                {
                    CheatMessageService.Message(
                        "CheatMenu.PawnGivePsylink.Message.NoBrain".Translate(pawn.LabelShortCap),
                        MessageTypeDefOf.RejectInput,
                        false);
                    return;
                }

                hediffLevel = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, brain) as Hediff_Level;
                pawn.health.AddHediff(hediffLevel);
            }

            int clampedLevel = Math.Min(selectedLevel, GetMaxPsylinkLevel());
            hediffLevel.ChangeLevel(clampedLevel - hediffLevel.level);

            DebugActionsUtility.DustPuffFrom(pawn);
            CheatMessageService.Message(
                "CheatMenu.PawnGivePsylink.Message.Result".Translate(pawn.LabelShortCap, clampedLevel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static int GetMaxPsylinkLevel()
        {
            HediffDef psylinkDef = HediffDefOf.PsychicAmplifier;
            return (int)psylinkDef.maxSeverity;
        }
    }
}
