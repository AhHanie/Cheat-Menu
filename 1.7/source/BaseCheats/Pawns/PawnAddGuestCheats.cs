using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public static class PawnAddGuestCheats
    {
        public static void Register()
        {
            RegisterAddSlave();
            RegisterAddPrisoner();
            RegisterAddGuest();
        }

        private static void RegisterAddSlave()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddSlave",
                "CheatMenu.Cheat.PawnAddSlave.Label",
                "CheatMenu.Cheat.PawnAddSlave.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .RequireIdeology()
                    .AddAction(context => AddGuest(GuestStatus.Slave)));
        }

        private static void RegisterAddPrisoner()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddPrisoner",
                "CheatMenu.Cheat.PawnAddPrisoner.Label",
                "CheatMenu.Cheat.PawnAddPrisoner.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(context => AddGuest(GuestStatus.Prisoner)));
        }

        private static void RegisterAddGuest()
        {
            CheatRegistry.Register(
                "CheatMenu.Base.PawnAddGuest",
                "CheatMenu.Cheat.PawnAddGuest.Label",
                "CheatMenu.Cheat.PawnAddGuest.Description",
                builder => builder
                    .InCategory("CheatMenu.Category.Pawns")
                    .AllowedIn(CheatAllowedGameStates.PlayingOnMap)
                    .RequireMap()
                    .AddAction(context => AddGuest(GuestStatus.Guest)));
        }

        private static void AddGuest(GuestStatus guestStatus)
        {
            Map map = Find.CurrentMap;
            Building_Bed selectedBed = FindCandidateBed(map, guestStatus);
            if (selectedBed == null)
            {
                CheatMessageService.Message(
                    "CheatMenu.PawnAddGuest.Message.NoBed".Translate(GetGuestStatusLabel(guestStatus)),
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            PawnKindDef pawnKindDef = SelectPawnKindForGuestStatus(guestStatus);
            Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
            GenSpawn.Spawn(pawn, selectedBed.Position, map);
            StripPawnGearAndInventory(pawn);

            pawn.ownership.ClaimBedIfNonMedical(selectedBed);
            pawn.guest.SetGuestStatus(Faction.OfPlayer, guestStatus);

            CheatMessageService.Message(
                "CheatMenu.PawnAddGuest.Message.Result".Translate(pawn.LabelShortCap, GetGuestStatusLabel(guestStatus), selectedBed.LabelCap),
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        private static Building_Bed FindCandidateBed(Map map, GuestStatus guestStatus)
        {
            foreach (Building_Bed bed in map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
            {
                bool bedHasNoFreeSlot = bed.OwnersForReading.Any() && !bed.AnyUnownedSleepingSlot;
                bool requiresPrisonerBed = guestStatus == GuestStatus.Prisoner && !bed.ForPrisoners;
                bool requiresSlaveBed = guestStatus == GuestStatus.Slave && !bed.ForSlaves;
                if (bedHasNoFreeSlot || requiresPrisonerBed || requiresSlaveBed)
                {
                    continue;
                }

                return bed;
            }

            return null;
        }

        private static PawnKindDef SelectPawnKindForGuestStatus(GuestStatus guestStatus)
        {
            if (guestStatus == GuestStatus.Guest)
            {
                return PawnKindDefOf.SpaceRefugee;
            }

            List<PawnKindDef> candidates = DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(pk => pk?.defaultFactionDef != null
                             && !pk.defaultFactionDef.isPlayer
                             && pk.RaceProps.Humanlike
                             && pk.mutant == null)
                .ToList();

            return candidates.RandomElement();
        }

        private static void StripPawnGearAndInventory(Pawn pawn)
        {
            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading.ToList())
            {
                if (pawn.equipment.TryDropEquipment(equipment, out ThingWithComps droppedEquipment, pawn.Position) && droppedEquipment != null)
                {
                    droppedEquipment.Destroy();
                }
            }

            pawn.inventory.innerContainer.Clear();
        }

        private static string GetGuestStatusLabel(GuestStatus guestStatus)
        {
            switch (guestStatus)
            {
                case GuestStatus.Slave:
                    return "CheatMenu.PawnAddGuest.Status.Slave".Translate();
                case GuestStatus.Prisoner:
                    return "CheatMenu.PawnAddGuest.Status.Prisoner".Translate();
                default:
                    return "CheatMenu.PawnAddGuest.Status.Guest".Translate();
            }
        }
    }
}
