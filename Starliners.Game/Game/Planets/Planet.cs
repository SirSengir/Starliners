/*
* Copyright (c) 2014 SirSengir
* Starliners (http://github.com/SirSengir/Starliners)
*
* This file is part of Starliners.
*
* Starliners is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* Starliners is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Starliners.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Runtime.Serialization;
using BLibrary.Serialization;
using BLibrary.Util;
using Starliners.Game.Forces;
using System.Collections.Generic;
using System.Linq;
using BLibrary.Network;
using Starliners.Network;
using Starliners.Game.Notifications;

namespace Starliners.Game.Planets {
    [Serializable]
    public sealed class Planet : StateObject, ILevyProvider {
        #region Constants

        public const int LOYALITY_MAX = 1000000;
        const string NAME_PREFIX = "planet";

        public const string STATS_MAINTENANCE_FRIGATES = "MaintenanceFrigates";
        public const string STATS_MAINTENANCE_DESTROYERS = "MaintenanceDestroyers";
        public const string STATS_MAINTENANCE_CRUISERS = "MaintenanceCruisers";
        public const string STATS_MAINTENANCE_BATTLESHIPS = "MaintenanceBattleships";
        public const string STATS_MAINTENANCE_DREADNOUGHTS = "MaintenanceDreadnoughts";

        public const string STATS_COMPETENCE_FRIGATES = "CompetenceFrigates";
        public const string STATS_COMPETENCE_DESTROYERS = "CompetenceDestroyers";
        public const string STATS_COMPETENCE_CRUISERS = "CompetenceCruisers";
        public const string STATS_COMPETENCE_BATTLESHIPS = "CompetenceBattleships";
        public const string STATS_COMPETENCE_DREADNOUGHTS = "CompetenceDreadnoughts";

        public const string STATS_COMPETENCE_HEAT = "CompetenceHeat";
        public const string STATS_COMPETENCE_KINETIC = "CompetenceKinetic";
        public const string STATS_COMPETENCE_RADIATION = "CompetenceRadiation";

        public const string STATS_COMPETENCE_SHIELD_REGEN = "CompetenceShieldRegen";
        public const string STATS_COMPETENCE_ARMOUR_REPAIR = "CompetenceArmourRepair";
        public const string STATS_COMPETENCE_HULL_REPAIR = "CompetenceHullRepair";

        public const string STATS_MODIFIER_RESIST_HEAT = "ModResistHeat";
        public const string STATS_MODIFIER_RESIST_KINETIC = "ModResistKinetic";
        public const string STATS_MODIFIER_RESIST_RADIATION = "ModResistRadiation";

        public const string STATS_MODIFIER_FOCUS_HEAT = "ModFocusHeat";
        public const string STATS_MODIFIER_FOCUS_KINETIC = "ModFocusKinetic";
        public const string STATS_MODIFIER_FOCUS_RADIATION = "ModFocusRadiation";

        public const string STATS_BUILD_LOGISTICS = "BuildLogistics";

        public const string STATS_INCOME_MINING = "IncomeMining";

        public static readonly StatsRecorder<int>.StatisticSlot[] INFO_SLOTS = new StatsRecorder<int>.StatisticSlot[] {
            new StatsRecorder<int>.StatisticSlot ("maintenance", STATS_MAINTENANCE_FRIGATES),
            new StatsRecorder<int>.StatisticSlot ("maintenance", STATS_MAINTENANCE_DESTROYERS),
            new StatsRecorder<int>.StatisticSlot ("maintenance", STATS_MAINTENANCE_CRUISERS),
            new StatsRecorder<int>.StatisticSlot ("maintenance", STATS_MAINTENANCE_BATTLESHIPS),
            new StatsRecorder<int>.StatisticSlot ("maintenance", STATS_MAINTENANCE_DREADNOUGHTS),

            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_FRIGATES),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_DESTROYERS),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_CRUISERS),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_BATTLESHIPS),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_DREADNOUGHTS),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_HEAT),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_KINETIC),
            new StatsRecorder<int>.StatisticSlot ("competence", STATS_COMPETENCE_RADIATION),

            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_RESIST_HEAT),
            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_RESIST_KINETIC),
            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_RESIST_RADIATION),
            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_FOCUS_HEAT),
            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_FOCUS_KINETIC),
            new StatsRecorder<int>.StatisticSlot ("modifier", STATS_MODIFIER_FOCUS_RADIATION),

            new StatsRecorder<int>.StatisticSlot ("production", STATS_BUILD_LOGISTICS),
            new StatsRecorder<int>.StatisticSlot ("production", STATS_INCOME_MINING)
        };

        #endregion

        [GameData (Remote = true, Key = "Type")]
        public PlanetType Type {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Owner")]
        public Faction Owner {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Location")]
        public Vect2d Location {
            get;
            set;
        }

        /// <summary>
        /// Gets the planet's current state.
        /// </summary>
        /// <value>The state.</value>
        public PlanetState State {
            get {
                PlanetState state = PlanetState.None;
                if (Orbit.HasRelatedFleets (FactionRelation.Hostile, Owner) && !Orbit.HasRelatedFleets (FactionRelation.Allied, Owner)) {
                    state |= PlanetState.Blockaded;
                }
                if (Loyality < LOYALITY_MAX / 10) {
                    state |= PlanetState.Unruly;
                }
                return state;
            }
        }

        [GameData (Remote = true, Key = "FullName")]
        public string FullName {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Skin")]
        public string Skin {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Size")]
        public int Size {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Culture")]
        public Culture Culture {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Levy")]
        public Levy Levy {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Orbit")]
        public Orbit Orbit {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Population")]
        public int Population {
            get;
            private set;
        }

        public int Loyality {
            get {
                return _loyality;
            }
            private set {
                _loyality = value;
                _loyality = _loyality < 0 ? 0 : _loyality > LOYALITY_MAX ? LOYALITY_MAX : _loyality;
            }
        }

        public uint Reenforcement {
            get {
                return 500 + (uint)Attributes [STATS_BUILD_LOGISTICS];
            }
        }

        public StatsRecorder<int> Attributes {
            get {
                return _attributes;
            }
        }

        public IReadOnlyDictionary<ulong, BuildingSector> Sectors {
            get {
                return _sectors;
            }
        }

        [GameData (Remote = true, Key = "Attributes")]
        StatsRecorder<int> _attributes = new StatsRecorder<int> ();
        [GameData (Remote = true, Key = "BuildingSectors")]
        Dictionary<ulong, BuildingSector> _sectors = new Dictionary<ulong, BuildingSector> ();
        [GameData (Remote = true, Key = "Loyality")]
        int _loyality;

        public Planet (IWorldAccess access, PlanetType type, Faction owner, string name, int size, Culture culture, int population)
            : base (access, FileUtils.CreateFileName (name)) {

            IsTickable = true;
            Type = type;
            Owner = owner;
            FullName = name;
            Skin = Type.ToString ().ToLowerInvariant () + access.Seed.Next (PlanetTypes.SKINS [Type]).ToString ();
            Size = size;
            Culture = culture;
            Population = population;
            Loyality = access.GetParameter<int> (ParameterKeys.EMPIRE_LOYALITY) * LOYALITY_MAX / 100;

            Levy = new Levy (access, name, this);
            Access.Controller.QueueState (Levy);

            Orbit = new Orbit (Access);
        }

        #region Serialization

        public Planet (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        protected override void OnCommissioned () {
            base.OnCommissioned ();
            RefreshAttributes ();
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            Orbit.CheckBattle ();
            PlanetState state = State;

            // Fly attacks.
            if (state.HasFlag (PlanetState.Blockaded)) {
                for (int i = 0; i < Orbit.Fleets.Count; i++) {
                    if (Orbit.Fleets [i].DetermineRelation (Owner) == FactionRelation.Hostile) {
                        Attack (Orbit.Fleets [i]);
                        // Spawn some 'splosions.
                        if (Access.Rand.NextDouble () < 0.1) {
                            Vect2d location = Location + new Vect2d (Access.Rand.NextDouble () * 2, Access.Rand.NextDouble () * 2) - new Vect2d (1, 1);
                            Access.Controller.SpawnParticle (new Particle (Access, location, ParticleId.Explosion) {
                                MaxAge = Access.Rand.Next (3) + 2,
                                Seed = Access.Rand.Next (4)
                            });
                        }
                    }
                }
            }

            if (ticks.HasFlag (TickType.Partial)) {
                // Rebel if we dropped low enough without being attacked.
                if (Loyality < LOYALITY_MAX / 100) {
                    Rebel ();
                }

                // Regenerate levy if possible.
                if (!state.HasFlag (PlanetState.Blockaded)
                    && !state.HasFlag (PlanetState.Unruly)) {
                    Levy.ReenforcementTick ();
                }
            }

            if (ticks.HasFlag (TickType.Rotation)) {
                if (!state.HasFlag (PlanetState.Blockaded) && !state.HasFlag (PlanetState.Unruly) && _attributes [STATS_INCOME_MINING] > 0) {
                    Owner.AssignIncome ("mining", _attributes [STATS_INCOME_MINING]);
                }
            }
        }

        void RefreshAttributes () {
            _attributes.Clear ();
            foreach (BuildingSector sector in _sectors.Values) {
                foreach (KeyValuePair<string, int> effect in sector.Improvement.Effects) {
                    _attributes.NoteStat (effect.Key, effect.Value * sector.Amount);
                }
            }
            MarkUpdated ();
        }

        void Rebel () {
            // Rebels just bounce back to loyal state.
            if (Owner.Flags.Contains (FactionFlags.REBEL)) {
                Loyality = LOYALITY_MAX;
                return;
            }
            // Create a rebel faction if needed and switch allegiance.
            string ident = Utils.BuildName ("rebels", Name);
            string search = Utils.BuildName (Faction.NAME_PREFIX, ident);
            Faction rebels = Access.States.Values.OfType<Faction> ().Where (p => string.Equals (search, p.Name)).FirstOrDefault ();
            if (rebels == null) {
                rebels = new Faction (Access, ident, Access.Assets.Values.OfType<FactionPreset> ().Where (p => p.Flags.Contains (FactionFlags.REBEL)).OrderBy (p => Access.Rand.Next ()).First ());
                Access.Controller.QueueState (rebels);
            }
            ChangeAllegiance (rebels);
        }

        /// <summary>
        /// Attacks the planet with the specified fleet.
        /// </summary>
        /// <param name="fleet">Fleet to attack with.</param>
        public void Attack (Fleet fleet) {
            Loyality -= fleet.Weight;
            if (Loyality > LOYALITY_MAX / 100) {
                return;
            }

            // Loyalty dropped to far, we switch allegiance.
            ChangeAllegiance (fleet.Owner);
        }

        void ChangeAllegiance (Faction changed) {
            Owner.Statistics.NoteStat (Faction.STAT_PLANETS_LOST, 1);
            changed.Statistics.NoteStat (Faction.STAT_PLANETS_CONQUERED, 1);
            changed.AssignLoot (ScoreKeeper.SCORE_CONQUEST_PLANETS, 100);

            // We are loyal to our new masters.
            Loyality = LOYALITY_MAX;

            // Reset the levy, destroying currently contained ships.
            Levy.Reset ();
            Levy.StandDown ();
            // Destroy some improvements
            List<Improvement> destroy = new List<Improvement> ();
            foreach (BuildingSector sector in _sectors.Values) {
                if (Access.Rand.NextDouble () < 0.4) {
                    destroy.Add (sector.Improvement);
                }
            }
            foreach (Improvement improvement in destroy) {
                RemoveImprovement (improvement);
            }

            NotificationManager.Instance.Notify (Owner, NotificationCategories.CONQUEST_REPORT, (ushort)GuiIds.Planet, this, new TextComposition ("planet_lost", new TextComponent (FullName)));

            Owner = changed;
            MarkUpdated (UpdateMarker.Update1);

            NotificationManager.Instance.Notify (changed, NotificationCategories.CONQUEST_REPORT, (ushort)GuiIds.Planet, this, new TextComposition ("planet_gained", new TextComponent (FullName)));

        }

        public int GetMaintenance (ShipSize size) {
            switch (size) {
                case ShipSize.Dreadnought:
                    return _attributes [STATS_MAINTENANCE_DREADNOUGHTS];
                case ShipSize.Battleship:
                    return _attributes [STATS_MAINTENANCE_BATTLESHIPS];
                case ShipSize.Cruiser:
                    return _attributes [STATS_MAINTENANCE_CRUISERS];
                case ShipSize.Destroyer:
                    return _attributes [STATS_MAINTENANCE_DESTROYERS];
                default:
                    return _attributes [STATS_MAINTENANCE_FRIGATES];
            }
        }

        int GetCompetence (ShipSize size) {
            switch (size) {
                case ShipSize.Dreadnought:
                    return _attributes [STATS_COMPETENCE_DREADNOUGHTS];
                case ShipSize.Battleship:
                    return _attributes [STATS_COMPETENCE_BATTLESHIPS];
                case ShipSize.Cruiser:
                    return _attributes [STATS_COMPETENCE_CRUISERS];
                case ShipSize.Destroyer:
                    return _attributes [STATS_COMPETENCE_DESTROYERS];
                default:
                    return _attributes [STATS_COMPETENCE_FRIGATES];
            }
        }

        public ShipModifiers CreateShipModifiers (ShipSize size) {
            return new ShipModifiers (
                _attributes [STATS_MODIFIER_RESIST_HEAT],
                _attributes [STATS_MODIFIER_RESIST_KINETIC],
                _attributes [STATS_MODIFIER_RESIST_RADIATION],
                _attributes [STATS_MODIFIER_FOCUS_HEAT],
                _attributes [STATS_MODIFIER_FOCUS_KINETIC],
                _attributes [STATS_MODIFIER_FOCUS_RADIATION]
            );
        }

        public int GetAttribute (string key) {
            return _attributes.Records.ContainsKey (key) ? _attributes.Records [key] : 0;
        }

        /// <summary>
        /// Adds the given improvement to this planet.
        /// </summary>
        /// <param name="building">Building.</param>
        public void AddImprovement (Improvement building) {
            BuildingSector sector;
            if (!_sectors.ContainsKey (building.Serial)) {
                sector = new BuildingSector (building);
                _sectors [building.Serial] = sector;
            } else {
                sector = _sectors [building.Serial];
            }

            if (building.Maximum <= 0 || sector.Amount < building.Maximum) {
                sector.Amount++;
            }
            RefreshAttributes ();
        }

        /// <summary>
        /// Removes one of the given improvement from this planet.
        /// </summary>
        /// <param name="building">Building.</param>
        public void RemoveImprovement (Improvement building) {
            if (!_sectors.ContainsKey (building.Serial)) {
                return;
            }

            _sectors [building.Serial].Amount--;
            if (_sectors [building.Serial].Amount <= 0) {
                _sectors.Remove (building.Serial);
            }
            RefreshAttributes ();
        }

        /// <summary>
        /// Determines the amount of the given improvement on the planet.
        /// </summary>
        /// <returns>The improvement.</returns>
        /// <param name="improvement">Improvement.</param>
        public int CountImprovement (Improvement improvement) {
            return _sectors.ContainsKey (improvement.Serial) ? _sectors [improvement.Serial].Amount : 0;
        }

        public List<BuildingSector> GetBuildingList () {
            List<BuildingSector> buildings = new List<BuildingSector> ();
            foreach (Improvement improvement in Access.Assets.Values.OfType<Improvement>().OrderBy(p => p.Categorized.Weight).ThenBy(p => p.Categorized.Name)) {

                if (!improvement.IsAvailable (this)) {
                    continue;
                }

                if (_sectors.ContainsKey (improvement.Serial)) {
                    buildings.Add (_sectors [improvement.Serial]);
                } else {
                    buildings.Add (new BuildingSector (improvement));
                }
            }
            return buildings;
        }

        public void OnShipLoss (ShipClass sclass) {
            Loyality -= sclass.Loot * 1000;
        }

        public void EnterOrbit (Fleet fleet) {
            Orbit.Insert (this, fleet);
            MarkUpdated (UpdateMarker.Update0);
        }

        public void LeaveOrbit (Fleet fleet) {
            Orbit.Leave (this, fleet);
            MarkUpdated (UpdateMarker.Update0);
        }

        public override void HandleNetworkUpdate (IPacketMarked packet) {
            if (packet.Marker == UpdateMarker.Update0) {
                Orbit = ((PacketUpdateStream)packet).DeserializeContent<Orbit> (Access);
            } else if (packet.Marker == UpdateMarker.Update1) {
                Owner = Access.RequireState<Faction> (((PacketUpdatePayload)packet).Payload.GetValue<ulong> (0));
                MarkUpdated ();
            } else {
                base.HandleNetworkUpdate (packet);
            }
        }

        public override IPacketMarked GetUpdatePacket () {
            if (UpdateSchedule.HasFlag (UpdateMarker.Update0)) {
                UnscheduleUpdate (UpdateMarker.Update0);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update0, Orbit);
            } else if (UpdateSchedule.HasFlag (UpdateMarker.Update1)) {
                UnscheduleUpdate (UpdateMarker.Update1);
                return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Update1, Owner.Serial);
            } else {
                return base.GetUpdatePacket ();
            }
        }

    }
}

