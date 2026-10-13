using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Cheat_Menu
{
    public static class QuestAcceptAllCheat
    {
        public static void Register()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.QuestAcceptAll",
                "CheatMenu.Cheat.QuestAcceptAll.Label",
                "CheatMenu.Cheat.QuestAcceptAll.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Quests")
                    .AllowedIn(CheatAllowedGameStates.Playing)
                    .AddAction(context => AcceptAllAvailableQuests()));
        }

        private static void AcceptAllAvailableQuests()
        {
            List<Quest> candidates = Find.QuestManager.questsInDisplayOrder
                .Where(quest => quest.State == QuestState.NotYetAccepted
                    && !quest.dismissed
                    && !quest.hidden
                    && !quest.hiddenInUI)
                .ToList();

            int acceptedCount = 0;
            foreach (Quest quest in candidates)
            {
                List<QuestPart_Choice> choiceParts = quest.PartsListForReading.OfType<QuestPart_Choice>().ToList();
                foreach (QuestPart_Choice choicePart in choiceParts)
                {
                    if (choicePart.choices.Any())
                    {
                        choicePart.Choose(choicePart.choices.RandomElement());
                    }
                }

                if (quest.State != QuestState.NotYetAccepted)
                {
                    continue;
                }

                Pawn accepter = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended
                    .Where(pawn => QuestUtility.CanPawnAcceptQuest(pawn, quest))
                    .RandomElementWithFallback();

                quest.Accept(accepter);
                SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                acceptedCount++;
            }

            CheatMessageService.Message(
                "CheatMenu.Cheat.QuestAcceptAll.Message.Result".Translate(acceptedCount),
                MessageTypeDefOf.TaskCompletion,
                false);
        }
    }
}
