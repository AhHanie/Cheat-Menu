using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private const string GeneralSetStuffContextKey = "BaseCheats.GeneralSetStuff.SelectedStuff";

        private static void RegisterSetStuff()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralSetStuff",
                "CheatMenu.General.SetStuff.Label",
                "CheatMenu.General.SetStuff.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddWindow(OpenStuffSelectionWindow)
                    .AddTool(
                        SetStuffAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat",
                        repeatTargeting: true));
        }

        private static void OpenStuffSelectionWindow(CheatExecutionContext context, Action continueFlow)
        {
            Find.WindowStack.Add(new GeneralSetStuffSelectionWindow(delegate (ThingDef selectedStuff)
            {
                context.Set(GeneralSetStuffContextKey, selectedStuff);
                continueFlow?.Invoke();
            }));
        }

        private static void SetStuffAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            if (!context.TryGet(GeneralSetStuffContextKey, out ThingDef selectedStuff))
            {
                CheatMessageService.Message("CheatMenu.General.SetStuff.Message.NoStuffSelected".Translate(), MessageTypeDefOf.RejectInput, false);
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

            int updatedCount = 0;
            for (int i = 0; i < thingsAtCell.Count; i++)
            {
                if (thingsAtCell[i].Stuff == null)
                {
                    continue;
                }

                SetThingStuff(thingsAtCell[i], selectedStuff);
                updatedCount++;
            }

            if (updatedCount == 0)
            {
                CheatMessageService.Message("CheatMenu.General.SetStuff.Message.NoStuffBearingThings".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            CheatMessageService.Message(
                "CheatMenu.General.SetStuff.Message.Result".Translate(updatedCount, selectedStuff.label),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static void SetThingStuff(Thing thing, ThingDef stuff)
        {
            float hitPointPercent = (float)thing.HitPoints / thing.MaxHitPoints;
            thing.SetStuffDirect(stuff);
            StatDefOf.MaxHitPoints.Worker.ClearCacheForThing(thing);
            thing.HitPoints = Mathf.CeilToInt(thing.MaxHitPoints * hitPointPercent);
            thing.Notify_ColorChanged();
            if (thing.Map != null)
            {
                thing.DirtyMapMesh(thing.Map);
            }

            if (thing is UnfinishedThing unfinishedThing)
            {
                ReplaceUnfinishedThingIngredients(unfinishedThing, stuff);
            }
        }

        private static void ReplaceUnfinishedThingIngredients(UnfinishedThing unfinishedThing, ThingDef stuff)
        {
            int ingredientCount = 0;
            for (int i = 0; i < unfinishedThing.ingredients.Count; i++)
            {
                ingredientCount += unfinishedThing.ingredients[i].stackCount;
            }

            unfinishedThing.ingredients.Clear();

            int guard = 100;
            while (ingredientCount > 0 && guard-- > 0)
            {
                Thing ingredient = ThingMaker.MakeThing(stuff);
                ingredient.stackCount = Mathf.Min(ingredientCount, stuff.stackLimit);
                unfinishedThing.ingredients.Add(ingredient);
                ingredientCount -= ingredient.stackCount;
            }
        }
    }
}
