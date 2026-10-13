using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterSetFactionRelations()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralSetFactionRelations",
                "CheatMenu.General.SetFactionRelations.Label",
                "CheatMenu.General.SetFactionRelations.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(OpenSetFactionRelationsMenu));
        }

        private static void OpenSetFactionRelationsMenu(CheatExecutionContext context)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Faction faction in Find.FactionManager.AllFactionsVisibleInViewOrder)
            {
                Faction localFaction = faction;
                foreach (FactionRelationKind value in Enum.GetValues(typeof(FactionRelationKind)))
                {
                    FactionRelationKind localRelationKind = value;
                    options.Add(new FloatMenuOption(
                        "CheatMenu.General.SetFactionRelations.Option".Translate(localFaction.ToString(), localRelationKind.ToString()),
                        delegate
                        {
                            if (localRelationKind == FactionRelationKind.Hostile)
                            {
                                Faction.OfPlayer.TryAffectGoodwillWith(localFaction, -100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
                            }
                            else if (localRelationKind == FactionRelationKind.Ally)
                            {
                                Faction.OfPlayer.TryAffectGoodwillWith(localFaction, 100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DebugGoodwill);
                            }
                        }));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}

