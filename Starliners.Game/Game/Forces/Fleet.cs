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
using BLibrary.Util;
using System.Runtime.Serialization;
using System.Collections.Generic;
using BLibrary.Serialization;
using Starliners.Graphics;
using BLibrary.Graphics;
using BLibrary.Network;
using Starliners.Network;

namespace Starliners.Game.Forces {

    [Serializable]
    public sealed class Fleet : StateObject, ISpriteDeclarant, IMobile {
        #region Constants

        const string NAME_PREFIX = "fleet";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fleet's location.
        /// </summary>
        /// <value>The location.</value>
        public Vect2d Location {
            get {
                return _location;
            }
            private set {
                _location = value;
                if (_gametoken != null) {
                    _gametoken.Location = _location;
                }
            }
        }

        /// <summary>
        /// Gets the fleet's current destination.
        /// </summary>
        /// <value>The destination.</value>
        [GameData (Remote = true, Key = "Destination")]
        public INavPoint Destination {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current movement of the fleet.
        /// </summary>
        /// <value>The movement.</value>
        [GameData (Remote = true, Key = "Movement")]
        public Movement Movement {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this fleet is moving.
        /// </summary>
        /// <value><c>true</c> if this instance is moving; otherwise, <c>false</c>.</value>
        public bool IsMoving {
            get {
                return Movement.Speed.X != 0 || Movement.Speed.Y != 0;
            }
        }

        /// <summary>
        /// Gets the fleet's state.
        /// </summary>
        /// <value>The state.</value>
        public FleetState State {
            get {
                if (Levies.Count <= 0) {
                    return FleetState.Depleted;
                }
                return _isEngaged ? FleetState.Engaged : FleetState.Available;
            }
        }

        /// <summary>
        /// Gets the port the fleet is currently located in or null.
        /// </summary>
        /// <value>The port.</value>
        [GameData (Remote = true, Key = "Port")]
        public INavPoint Port {
            get;
            private set;
        }

        /// <summary>
        /// Gets the full, human-readable name of the fleet.
        /// </summary>
        /// <value>The full name.</value>
        [GameData (Remote = true, Key = "FullName")]
        public string FullName {
            get;
            private set;
        }

        /// <summary>
        /// Gets the faction owning this fleet.
        /// </summary>
        /// <value>The owner.</value>
        public Faction Owner {
            get {
                return Backer.FleetOwner;
            }
        }

        /// <summary>
        /// Gets the fleet's backer. This is either a faction or an invasion wave.
        /// </summary>
        /// <value>The backer.</value>
        [GameData (Remote = true, Key = "Backer")]
        public IFleetBacker Backer {
            get;
            private set;
        }

        public int Weight {
            get {
                int weight = 0;
                foreach (Levy levy in Levies) {
                    weight += levy.Weight;
                }
                return weight;
            }
        }

        /// <summary>
        /// Gets the amount of ships (remaining) in the fleet.
        /// </summary>
        /// <value>The ship count.</value>
        public int ShipCount {
            get {
                int count = 0;
                foreach (Levy levy in Levies) {
                    count += levy.Ships.Count;
                }
                return count;
            }
        }

        public IReadOnlyList<Levy> Levies {
            get {
                return _levies.Listing;
            }
        }

        public ShipProjector Projector {
            get;
            private set;
        }

        #endregion

        #region Fields

        [GameData (Remote = true, Key = "Location")]
        Vect2d _location;
        [GameData (Remote = true, Key = "Levies")]
        IdList<Levy> _levies;
        [GameData (Remote = true, Key = "GameToken")]
        EntityFleet _gametoken;
        [GameData (Remote = true, Key = "IsEngaged")]
        bool _isEngaged;

        Pathing _pathing = new Pathing ();

        #endregion

        public Fleet (IWorldAccess access, string name, Vect2d location, IFleetBacker backer, IEnumerable<Levy> initial)
            : this (access, name, backer, initial) {

            Location = location;
        }

        public Fleet (IWorldAccess access, string name, INavPoint port, IFleetBacker backer, IEnumerable<Levy> initial)
            : this (access, name, backer, initial) {

            Port = port;
        }

        private Fleet (IWorldAccess access, string name, IFleetBacker backer, IEnumerable<Levy> initial)
            : base (access, Utils.BuildName (NAME_PREFIX, name)) {

            IsTickable = true;
            Backer = backer;
            _levies = new IdList<Levy> (Access);
            foreach (Levy levy in initial) {
                _levies.Add (levy);
            }
            FullName = Eponym.GetForWorld (Access).GenerateFleetName (this);
        }

        #region Serialization

        public Fleet (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        protected override void OnCommissioned () {
            base.OnCommissioned ();
            Projector = new ShipProjector ("fleet4", Backer.Colours.Vessels, Backer.Colours.Shields, Serial.GetHashCode ());
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            // Cull levies which have stood down.
            for (int i = 0; i < _levies.Listing.Count; i++) {
                if (_levies.Listing [0].IsRaised) {
                    continue;
                }
                _levies.Remove (_levies.Listing [0]);
                MarkUpdated (UpdateMarker.Update2);
            }

            // Kill depleted fleets.
            if (State == FleetState.Depleted) {
                Dissolve (false);
            }

            // Get the new movement (speed as a vector + angle).,
            Movement changed = _pathing.UpdateMovement (this);
            // If the movement has changed, we need to schedule an update for the interface.
            if (changed != Movement) {
                ScheduleUpdate (UpdateMarker.Update0);
                Movement = changed;
                // If we have stopped, we need to update movement client-side at once.
                if (!IsMoving) {
                    ForceNetUpdate = true;
                }
            }
            // Apply the current movement.
            if (Movement.Speed.X != 0 || Movement.Speed.Y != 0) {
                Location += Movement.Speed;
            }

        }

        public override void RenderTick () {
            base.RenderTick ();

            // Apply the current movement.
            if (Movement.Speed.X != 0 || Movement.Speed.Y != 0) {
                Location += Movement.Speed;
            }
        }

        /// <summary>
        /// Dissolves the fleet, sending still contained levies home.
        /// </summary>
        public void Dissolve (bool standdown) {
            if (Port != null) {
                Port.LeaveOrbit (this);
            }

            // Only issue stand down orders if it was requested.
            if (standdown) {
                foreach (Levy levy in _levies) {
                    levy.StandDown ();
                }
            }

            // Kill the token.
            if (_gametoken != null) {
                _gametoken.IsDead = true;
            }
            Backer.OnFleetDisolution (this);
            IsDead = true;
        }

        public void Relocate (INavPoint nav) {
            Destination = nav;
            if (Port != null) {
                Location = Port.Location;
                Port.LeaveOrbit (this);
                Port = null;
            }
            if (_gametoken == null) {
                _gametoken = Access.Controller.SpawnFleetEntity (this);
                ScheduleUpdate (UpdateMarker.Update1);
            }
            _gametoken.IsVisible = true;
            MarkUpdated ();
        }

        public void OnDestinationReached () {
            Location = Destination.Location;
            Destination.EnterOrbit (this);
            Port = Destination;
            _gametoken.IsVisible = false;
            Destination = null;
            MarkUpdated ();
        }

        public void RegisterIcons (IIconRegister register) {
            Projector.RegisterIcons (register);
        }

        public void PurgeDepletedLevies () {
            for (int i = 0; i < _levies.Listing.Count; i++) {

                if (_levies.Listing [0].State != LevyState.Depleted) {
                    continue;
                }

                Access.GameConsole.Debug ("Removing depleted levy {0} from fleet {1}.", _levies.Listing [0], this);
                _levies.Listing [0].StandDown ();
                _levies.Remove (_levies.Listing [0]);
                MarkUpdated (UpdateMarker.Update2);
            }
        }

        public void FlagEngaged (bool flag) {
            _isEngaged = flag;
            foreach (Levy levy in _levies) {
                levy.FlagEngaged (flag);
            }
            PurgeDepletedLevies ();
            MarkUpdated ();
        }

        /// <summary>
        /// Gets an array indicating the amount of ships by ship size in this fleet.
        /// </summary>
        /// <returns>The fleet composition.</returns>
        public int[] GetFleetComposition () {
            int[] counts = new int[ShipSizes.VALUES.Length - 1];
            foreach (Levy levy in Levies) {
                levy.Census (counts);
            }
            return counts;
        }

        /// <summary>
        /// Incorporates the specified other fleet into this one, dissolving the other fleet.
        /// </summary>
        /// <param name="other">Other.</param>
        public void Incorporate (Fleet other) {
            foreach (Levy levy in other.Levies) {
                _levies.Add (levy);
            }
            other.Dissolve (false);
            MarkUpdated ();
        }

        public FactionRelation DetermineRelation (Fleet other) {
            return DetermineRelation (other.Owner);
        }

        public FactionRelation DetermineRelation (Faction faction) {
            return Owner != faction ? FactionRelation.Hostile : FactionRelation.Allied;
        }

        #region Networking

        public override void HandleNetworkUpdate (IPacketMarked packet) {
            base.HandleNetworkUpdate (packet);
            if (packet.Marker == UpdateMarker.Update0) {
                Payload payload = ((PacketUpdatePayload)packet).Payload;
                Location = payload.GetValue<Vect2d> (0);
                Movement = new Movement (payload.GetValue<Vect2d> (1), payload.GetValue<double> (2));
            } else if (packet.Marker == UpdateMarker.Update1) {
                _gametoken = Access.RequireEntity<EntityFleet> (((PacketUpdatePayload)packet).Payload.GetValue<ulong> (0));
            } else if (packet.Marker == UpdateMarker.Update2) {
                _levies = ((PacketUpdateStream)packet).DeserializeContent<IdList<Levy>> (Access);
            }
        }

        public override IPacketMarked GetUpdatePacket () {
            if (UpdateSchedule.HasFlag (UpdateMarker.Update0)) {
                UnscheduleUpdate (UpdateMarker.Update0);
                IPacketMarked packet = new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Update0,
                                           Location, Movement.Speed, Movement.Angle);
                return packet;
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Update1)) {
                UnscheduleUpdate (UpdateMarker.Update1);
                return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Update1, _gametoken.Serial);
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Update2)) {
                UnscheduleUpdate (UpdateMarker.Update2);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update2, _levies);
            }
            return base.GetUpdatePacket ();
        }

        #endregion

    }
}

