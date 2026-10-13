using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterAddTraitToUniqueWeapon()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralAddTraitToUniqueWeapon",
                "CheatMenu.General.AddTraitToUniqueWeapon.Label",
                "CheatMenu.General.AddTraitToUniqueWeapon.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddTool(
                        OpenAddTraitToUniqueWeaponWindowAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void RegisterRemoveTraitFromUniqueWeapon()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralRemoveTraitFromUniqueWeapon",
                "CheatMenu.General.RemoveTraitFromUniqueWeapon.Label",
                "CheatMenu.General.RemoveTraitFromUniqueWeapon.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireOdyssey()
                    .AddTool(
                        OpenRemoveTraitFromUniqueWeaponWindowAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenAddTraitToUniqueWeaponWindowAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            CompUniqueWeapon comp = TryGetUniqueWeaponCompAtCell(target.Cell);
            if (comp == null)
            {
                CheatMessageService.Message("CheatMenu.General.UniqueWeaponTrait.Message.NoUniqueWeapon".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<WeaponTraitDef> options = DefDatabase<WeaponTraitDef>.AllDefsListForReading
                .Where(comp.CanAddTrait)
                .OrderBy(trait => trait.label)
                .ThenBy(trait => trait.defName)
                .ToList();
            if (options.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.General.AddTraitToUniqueWeapon.Message.NoTraitsAvailable".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new GeneralUniqueWeaponTraitSelectionWindow(
                options,
                selectedTrait =>
                {
                    comp.AddTrait(selectedTrait);
                    comp.Setup(fromSave: true);
                    CheatMessageService.Message(
                        "CheatMenu.General.AddTraitToUniqueWeapon.Message.Result".Translate(selectedTrait.LabelCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                },
                "CheatMenu.General.AddTraitToUniqueWeapon.Window.Title",
                "CheatMenu.General.UniqueWeaponTrait.Window.SearchTooltip",
                "CheatMenu.General.UniqueWeaponTrait.Window.NoMatches",
                "CheatMenu.General.UniqueWeaponTrait.Window.SelectButton",
                "CheatMenu.General.AddTraitToUniqueWeapon.SearchField"));
        }

        private static void OpenRemoveTraitFromUniqueWeaponWindowAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            CompUniqueWeapon comp = TryGetUniqueWeaponCompAtCell(target.Cell);
            if (comp == null)
            {
                CheatMessageService.Message("CheatMenu.General.UniqueWeaponTrait.Message.NoUniqueWeapon".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<WeaponTraitDef> options = comp.TraitsListForReading
                .OrderBy(trait => trait.label)
                .ThenBy(trait => trait.defName)
                .ToList();
            if (options.Count == 0)
            {
                CheatMessageService.Message("CheatMenu.General.RemoveTraitFromUniqueWeapon.Message.NoTraitsOnWeapon".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Find.WindowStack.Add(new GeneralUniqueWeaponTraitSelectionWindow(
                options,
                selectedTrait =>
                {
                    comp.TraitsListForReading.Remove(selectedTrait);
                    CheatMessageService.Message(
                        "CheatMenu.General.RemoveTraitFromUniqueWeapon.Message.Result".Translate(selectedTrait.LabelCap),
                        MessageTypeDefOf.PositiveEvent,
                        false);
                },
                "CheatMenu.General.RemoveTraitFromUniqueWeapon.Window.Title",
                "CheatMenu.General.UniqueWeaponTrait.Window.SearchTooltip",
                "CheatMenu.General.UniqueWeaponTrait.Window.NoMatches",
                "CheatMenu.General.UniqueWeaponTrait.Window.SelectButton",
                "CheatMenu.General.RemoveTraitFromUniqueWeapon.SearchField"));
        }

        private static CompUniqueWeapon TryGetUniqueWeaponCompAtCell(IntVec3 cell)
        {
            Thing thing = Find.CurrentMap.thingGrid.ThingsAt(cell).FirstOrDefault(x => x.HasComp<CompUniqueWeapon>());
            return thing?.TryGetComp<CompUniqueWeapon>();
        }
    }
}
