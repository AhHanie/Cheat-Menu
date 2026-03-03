using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public class CheatMenuMapComponent : MapComponent
    {
        private const int TickInterval = 300;
        private const int InstantGrowTickInterval = 600;
        private const int InfiniteOrbitalTradersTickInterval = 3600;
        private static List<Pawn> cachedMapPawns;

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

            cachedMapPawns = map.mapPawns.FreeColonistsSpawned;

            CheatMenuGameComponent gameComponent = Current.Game.GetComponent<CheatMenuGameComponent>();
            ApplyNeeds(gameComponent);
            ApplyInfinitePower(gameComponent);
            ApplyInstantGrowGrowingZones(gameComponent);
            ApplyInfiniteOrbitalTraders(gameComponent);
            ApplyVREAndroids(gameComponent, cachedMapPawns);

            if (ModsConfig.BiotechActive)
            {
                ApplyMechEnergy(gameComponent);
                ApplyDeathrest(gameComponent);
                ApplyHemogen(gameComponent);
            }

            if (ModsConfig.RoyaltyActive)
            {
                ApplyPsyfocus(gameComponent);
                ApplyPsychicEntropy(gameComponent);
            }
        }

        private void ApplyNeeds(CheatMenuGameComponent gameComponent)
        {
            for (int pawnIndex = 0; pawnIndex < cachedMapPawns.Count; pawnIndex++)
            {
                Pawn pawn = cachedMapPawns[pawnIndex];
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

        private void ApplyDeathrest(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsNeeds.InfiniteDeathrestKey))
            {
                return;
            }

            for (int i = 0; i < cachedMapPawns.Count; i++)
            {
                Pawn pawn = cachedMapPawns[i];
                if (pawn.needs == null)
                {
                    continue;
                }

                Need deathrest = pawn.needs.TryGetNeed(CheatMenuNeedDefOf.Deathrest);
                if (deathrest != null)
                {
                    deathrest.CurLevel = deathrest.MaxLevel;
                }
            }
        }

        private void ApplyHemogen(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsNeeds.InfiniteHemogenKey))
            {
                return;
            }

            for (int i = 0; i < cachedMapPawns.Count; i++)
            {
                GeneUtility.OffsetHemogen(cachedMapPawns[i], 1f);
            }
        }

        private void ApplyPsyfocus(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.InfinitePsyfocusKey))
            {
                return;
            }

            for (int i = 0; i < cachedMapPawns.Count; i++)
            {
                ApplyPsyfocus(cachedMapPawns[i]);
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

        private void ApplyPsychicEntropy(CheatMenuGameComponent gameComponent)
        {
            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.ClearPsychicEntropyKey))
            {
                return;
            }

            for (int i = 0; i < cachedMapPawns.Count; i++)
            {
                Pawn_PsychicEntropyTracker psychicEntropy = cachedMapPawns[i].psychicEntropy;
                psychicEntropy?.RemoveAllEntropy();
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

        private void ApplyInstantGrowGrowingZones(CheatMenuGameComponent gameComponent)
        {
            if (Find.TickManager.TicksGame % InstantGrowTickInterval != 0)
            {
                return;
            }

            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.InstantGrowGrowingZonesKey))
            {
                return;
            }

            List<Zone> zones = map.zoneManager.AllZones;
            for (int zoneIndex = 0; zoneIndex < zones.Count; zoneIndex++)
            {
                Zone_Growing growingZone = zones[zoneIndex] as Zone_Growing;
                if (growingZone == null)
                {
                    continue;
                }

                List<IntVec3> cells = growingZone.Cells;
                for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                {
                    Plant plant = cells[cellIndex].GetPlant(map);
                    if (plant == null || plant.def.plant == null)
                    {
                        continue;
                    }

                    int growthRemaining = (int)((1f - plant.Growth) * plant.def.plant.growDays);
                    plant.Age += growthRemaining;
                    plant.Growth = 1f;
                }
            }
        }

        private void ApplyInfiniteOrbitalTraders(CheatMenuGameComponent gameComponent)
        {
            if (Find.TickManager.TicksGame % InfiniteOrbitalTradersTickInterval != 0)
            {
                return;
            }

            if (!gameComponent.IsEnabled(ToggleCheatsGeneral.InfiniteOrbitalTradersKey))
            {
                return;
            }

            List<PassingShip> passingShips = map.passingShipManager.passingShips;
            for (int i = 0; i < passingShips.Count; i++)
            {
                if (passingShips[i] is TradeShip)
                {
                    return;
                }
            }

            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentDefOf.OrbitalTraderArrival.category, map);
            incidentParms.forced = true;
            IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(incidentParms);
        }

        private void ApplyVREAndroids(CheatMenuGameComponent gameComponent, List<Pawn> pawns)
        {
            if (!ModsConfig.IsActive(VREAndroidsToggleCheats.PackageId))
            {
                return;
            }

            VREAndroidsToggleCheats.Apply(gameComponent, pawns);
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


