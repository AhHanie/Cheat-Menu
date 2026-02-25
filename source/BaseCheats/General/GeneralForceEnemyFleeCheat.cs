using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Cheat_Menu
{
    public static partial class GeneralCheats
    {
        private static void RegisterForceEnemyFlee()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.GeneralForceEnemyFlee",
                "CheatMenu.Cheat.GeneralForceEnemyFlee.Label",
                "CheatMenu.Cheat.GeneralForceEnemyFlee.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.General")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(ForceEnemyFlee));
        }

        private static void ForceEnemyFlee(CheatExecutionContext context)
        {
            Map map = Find.CurrentMap;
            List<Lord> lords = map.lordManager.lords;
            int forcedCount = 0;
            for (int i = 0; i < lords.Count; i++)
            {
                Lord lord = lords[i];
                if (lord.faction == null || !lord.faction.HostileTo(Faction.OfPlayer) || !lord.faction.def.autoFlee)
                {
                    continue;
                }

                LordToil panicFleeToil = lord.Graph.lordToils.FirstOrDefault(toil => toil is LordToil_PanicFlee);
                if (panicFleeToil == null)
                {
                    continue;
                }

                lord.GotoToil(panicFleeToil);
                forcedCount++;
            }

            CheatMessageService.Message(
                forcedCount > 0
                    ? "CheatMenu.GeneralForceEnemyFlee.Message.Result".Translate(forcedCount)
                    : "CheatMenu.GeneralForceEnemyFlee.Message.NoneFound".Translate(),
                forcedCount > 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent,
                false);
        }
    }
}
