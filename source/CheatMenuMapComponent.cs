using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public class CheatMenuMapComponent : MapComponent
    {
        private const int TickInterval = 300;

        private static readonly List<NeedEntry> NeedToggleEntries = new List<NeedEntry>
        {
            new NeedEntry(ToggleCheatsNeeds.InfiniteJoyKey, CheatMenuNeedDefOf.Joy),
            new NeedEntry(ToggleCheatsNeeds.InfiniteFoodKey, CheatMenuNeedDefOf.Food),
            new NeedEntry(ToggleCheatsNeeds.InfiniteRestKey, CheatMenuNeedDefOf.Rest),
            new NeedEntry(ToggleCheatsNeeds.InfiniteIndoorsKey, CheatMenuNeedDefOf.Indoors),
            new NeedEntry(ToggleCheatsNeeds.InfiniteBeautyKey, CheatMenuNeedDefOf.Beauty),
            new NeedEntry(ToggleCheatsNeeds.InfiniteComfortKey, CheatMenuNeedDefOf.Comfort),
            new NeedEntry(ToggleCheatsNeeds.InfiniteOutdoorsKey, CheatMenuNeedDefOf.Outdoors),
            new NeedEntry(ToggleCheatsNeeds.InfiniteMoodKey, CheatMenuNeedDefOf.Mood)
        };

        public CheatMenuMapComponent(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % TickInterval != 0)
            {
                return;
            }

            CheatMenuGameComponent gameComponent = Current.Game.GetComponent<CheatMenuGameComponent>();
            ApplyNeeds(gameComponent);
            ApplyInfinitePower(gameComponent);

            if (ModsConfig.BiotechActive)
            {
                ApplyMechEnergy(gameComponent);
            }

            if (ModsConfig.RoyaltyActive)
            {
                ApplyPsyfocus(gameComponent);
            }
        }

        private void ApplyNeeds(CheatMenuGameComponent gameComponent)
        {
            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;

            for (int pawnIndex = 0; pawnIndex < colonists.Count; pawnIndex++)
            {
                Pawn pawn = colonists[pawnIndex];
                if (pawn.needs == null)
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < NeedToggleEntries.Count; entryIndex++)
                {
                    NeedEntry entry = NeedToggleEntries[entryIndex];
                    if (!gameComponent.IsEnabled(entry.toggleCheatKey))
                    {
                        continue;
                    }

                    Need need = pawn.needs.TryGetNeed(entry.need);
                    if (need != null)
                    {
                        need.CurLevel = need.MaxLevel;
                    }
                }
            }
        }

        private void ApplyMechEnergy(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsNeeds.InfiniteMechEnergyKey))
            {
                return;
            }

            List<Pawn> mechs = map.mapPawns.SpawnedColonyMechs;
            for (int i = 0; i < mechs.Count; i++)
            {
                Pawn mech = mechs[i];
                if (mech.needs == null)
                {
                    continue;
                }

                Need mechEnergy = mech.needs.TryGetNeed(CheatMenuNeedDefOf.MechEnergy);
                if (mechEnergy != null)
                {
                    mechEnergy.CurLevel = mechEnergy.MaxLevel;
                }
            }
        }

        private void ApplyPsyfocus(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.InfinitePsyfocusKey))
            {
                return;
            }

            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                ApplyPsyfocus(colonists[i]);
            }
        }

        private static void ApplyPsyfocus(Pawn pawn)
        {
            Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
            if (psychicEntropy != null && psychicEntropy.Psylink != null)
            {
                float num = 1f - psychicEntropy.CurrentPsyfocus;
                if (num > 0.001f)
                {
                    psychicEntropy.OffsetPsyfocusDirectly(num);
                }
            }
        }
        private void ApplyInfinitePower(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.InfinitePowerKey))
            {
                return;
            }

            List<Building> buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                CompPowerTrader powerComp = building.TryGetComp<CompPowerTrader>();
                if (powerComp == null || powerComp.PowerOn)
                {
                    continue;
                }

                if (!FlickUtility.WantsToBeOn(building) || building.IsBrokenDown())
                {
                    continue;
                }

                powerComp.PowerOn = true;
            }
        }

        private class NeedEntry
        {
            public NeedDef need;
            public string toggleCheatKey;

            public NeedEntry(string toggleCheatKey, NeedDef need)
            {
                this.toggleCheatKey = toggleCheatKey;
                this.need = need;
            }
        }
    }
}


