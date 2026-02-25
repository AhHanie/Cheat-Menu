using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterChangeThingStyle()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralChangeThingStyle",
                "CheatMenu.Cheat.GeneralChangeThingStyle.Label",
                "CheatMenu.Cheat.GeneralChangeThingStyle.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddTool(
                        ChangeThingStyleAtTargetCell,
                        CreateCellTargetingParameters,
                        "CheatMenu.Shared.Message.SelectCellForCheat"));
        }

        private static void ChangeThingStyleAtTargetCell(CheatExecutionContext context, LocalTargetInfo target)
        {
            Map map = Find.CurrentMap;
            IntVec3 cell = target.Cell;
            if (map == null || !cell.IsValid || !cell.InBounds(map))
            {
                CheatMessageService.Message("CheatMenu.Shared.Message.InvalidCell".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Thing thing = map.thingGrid.ThingsAt(cell).FirstOrDefault(x => x != null && x.def.CanBeStyled());
            if (thing == null)
            {
                CheatMessageService.Message("CheatMenu.GeneralChangeThingStyle.Message.NoStyleableThing".Translate(), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            List<ThingStyleDef> availableStyles = CollectAvailableStyles(thing);
            if (availableStyles.Count == 0)
            {
                CheatMessageService.Message(
                    "CheatMenu.GeneralChangeThingStyle.Message.NoStylesAvailable".Translate(thing.LabelCap),
                    MessageTypeDefOf.NeutralEvent,
                    false);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            AddStyleOption(
                options,
                thing,
                () => null,
                "CheatMenu.GeneralChangeThingStyle.Option.Standard".Translate());
            AddStyleOption(
                options,
                thing,
                () => availableStyles.RandomElementByWeight(style => style != thing.StyleDef ? 1f : 0.01f),
                "CheatMenu.GeneralChangeThingStyle.Option.Random".Translate());

            for (int i = 0; i < availableStyles.Count; i++)
            {
                ThingStyleDef capturedStyle = availableStyles[i];
                AddStyleOption(options, thing, () => capturedStyle, capturedStyle.defName);
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        private static List<ThingStyleDef> CollectAvailableStyles(Thing thing)
        {
            List<ThingStyleDef> styles = new List<ThingStyleDef>();
            HashSet<ThingStyleDef> seen = new HashSet<ThingStyleDef>();

            List<ThingStyleChance> randomStyles = thing?.def?.randomStyle;
            if (!randomStyles.NullOrEmpty())
            {
                for (int i = 0; i < randomStyles.Count; i++)
                {
                    ThingStyleDef styleDef = randomStyles[i]?.StyleDef;
                    if (styleDef?.graphicData != null && seen.Add(styleDef))
                    {
                        styles.Add(styleDef);
                    }
                }
            }

            foreach (StyleCategoryDef category in DefDatabase<StyleCategoryDef>.AllDefsListForReading)
            {
                if (category?.thingDefStyles == null || !category.thingDefStyles.Any(y => y.ThingDef == thing.def))
                {
                    continue;
                }

                ThingStyleDef styleDef = category.GetStyleForThingDef(thing.def);
                if (styleDef != null && seen.Add(styleDef))
                {
                    styles.Add(styleDef);
                }
            }

            return styles;
        }

        private static void AddStyleOption(List<DebugMenuOption> options, Thing thing, Func<ThingStyleDef> styleSelector, string label)
        {
            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
            {
                thing.StyleDef = styleSelector();
                if (thing.Map != null)
                {
                    thing.DirtyMapMesh(thing.Map);
                }
            }));
        }
    }
}
