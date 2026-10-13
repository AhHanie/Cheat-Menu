using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Cheat_Menu
{
    public static class Infusion2Cheats
    {
        private const string CategoryKey = "CheatMenu.Category.Infusion2";
        private const string SelectedInfusionContextKey = "ModCompat.Infusion2.SelectedInfusion";
        private static bool registered;
        private static List<Def> cachedActiveInfusions = null;

        public static void Register()
        {
            if (registered)
            {
                return;
            }

            registered = true;
            RegisterInfuseCheat();
            RegisterRemoveInfusionCheat();
            RegisterRemoveAllInfusionsCheat();
        }

        private static void RegisterInfuseCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.Infusion2.Infuse",
                "CheatMenu.Infusion2.Cheat.Infuse.Label",
                "CheatMenu.Infusion2.Cheat.Infuse.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenInfusionSelectionWindow)
                    .AddTool(
                        InfuseAtTarget,
                        CreateTargetingParameters,
                        "CheatMenu.Infusion2.Infuse.Message.SelectThing"));
        }

        private static void RegisterRemoveInfusionCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.Infusion2.RemoveInfusion",
                "CheatMenu.Infusion2.Cheat.RemoveInfusion.Label",
                "CheatMenu.Infusion2.Cheat.RemoveInfusion.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        OpenRemoveInfusionWindowAtTarget,
                        CreateTargetingParameters,
                        "CheatMenu.Infusion2.RemoveInfusion.Message.SelectThing"));
        }

        private static void RegisterRemoveAllInfusionsCheat()
        {
            CheatRegistry.Register(
                "CheatMenu.ModCompat.Infusion2.RemoveAllInfusions",
                "CheatMenu.Infusion2.Cheat.RemoveAllInfusions.Label",
                "CheatMenu.Infusion2.Cheat.RemoveAllInfusions.Description",
                builder => builder
                    .InCategory(CategoryKey)
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        RemoveAllInfusionsAtTarget,
                        CreateTargetingParameters,
                        "CheatMenu.Infusion2.RemoveAllInfusions.Message.SelectThing"));
        }

        private static void OpenInfusionSelectionWindow(CheatExecutionContext context, System.Action continueFlow)
        {
            if (cachedActiveInfusions == null)
            {
                cachedActiveInfusions = Infusion2Reflection.GetActiveInfusionDefs();
            }
            if (cachedActiveInfusions.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Infusion2.Infuse.Message.NoneAvailable".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new Infusion2InfuseSelectionWindow(
                    cachedActiveInfusions,
                    selectedDef =>
                    {
                        context.Set(SelectedInfusionContextKey, selectedDef);
                        continueFlow?.Invoke();
                    }));
        }

        private static TargetingParameters CreateTargetingParameters(CheatExecutionContext context)
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

        private static void InfuseAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(SelectedInfusionContextKey, out Def selectedInfusion))
            {
                CheatMessageService.Message("CheatMenu.Infusion2.Infuse.Message.NoneSelected".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Thing foundThing = null;
            object compInfusion = null;

            foreach (Thing thingAtCell in thingsAtCell)
            {
                Logger.Message($"DEF: {thingAtCell.def}");
                compInfusion = Infusion2Reflection.GetCompInfusion(thingAtCell);
                if (compInfusion != null)
                {
                    foundThing = thingAtCell;
                    break;
                }
            }

            if (foundThing == null)
            {
                CheatMessageService.Message("CheatMenu.Infusion2.Message.InvalidTarget".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (!Infusion2Reflection.AddInfusion(compInfusion, selectedInfusion))
            {
                CheatMessageService.Message("CheatMenu.Infusion2.Message.OperationFailed".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            string infusionLabel = Infusion2Reflection.GetInfusionLabel(selectedInfusion);
            CheatMessageService.Message(
                "CheatMenu.Infusion2.Infuse.Message.Result".Translate(foundThing.LabelShortCap, infusionLabel),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void OpenRemoveInfusionWindowAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Thing foundThing = null;
            object compInfusion = null;

            foreach (Thing thingAtCell in thingsAtCell)
            {
                compInfusion = Infusion2Reflection.GetCompInfusion(thingAtCell);
                if (compInfusion != null)
                {
                    foundThing = thingAtCell;
                    break;
                }
            }

            List<Def> infusions = Infusion2Reflection.GetInfusions(compInfusion);
            if (infusions.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Infusion2.RemoveInfusion.Message.NoneOnTarget".Translate(foundThing.LabelShortCap), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.WindowStack.Add(
                new Infusion2RemoveInfusionSelectionWindow(
                    infusions,
                    selectedDef =>
                    {
                        if (!Infusion2Reflection.RemoveInfusion(compInfusion, selectedDef))
                        {
                            CheatMessageService.Message("CheatMenu.Infusion2.Message.OperationFailed".Translate(), MessageTypeDefOf.RejectInput, false);
                            return;
                        }

                        string infusionLabel = Infusion2Reflection.GetInfusionLabel(selectedDef);
                        CheatMessageService.Message(
                            "CheatMenu.Infusion2.RemoveInfusion.Message.Result".Translate(foundThing.LabelShortCap, infusionLabel),
                            MessageTypeDefOf.PositiveEvent,
                            false);
                    }));
        }

        private static void RemoveAllInfusionsAtTarget(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            List<Thing> thingsAtCell = map.thingGrid.ThingsAt(cell).ToList();
            if (thingsAtCell.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.NoThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Thing foundThing = null;
            object compInfusion = null;

            foreach (Thing thingAtCell in thingsAtCell)
            {
                compInfusion = Infusion2Reflection.GetCompInfusion(thingAtCell);
                if (compInfusion != null)
                {
                    foundThing = thingAtCell;
                    break;
                }
            }

            int removedCount = Infusion2Reflection.GetInfusions(compInfusion).Count;
            if (!Infusion2Reflection.RemoveAllInfusions(compInfusion))
            {
                CheatMessageService.Message("CheatMenu.Infusion2.Message.OperationFailed".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.Infusion2.RemoveAllInfusions.Message.Result".Translate(foundThing.LabelShortCap, removedCount),
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}
