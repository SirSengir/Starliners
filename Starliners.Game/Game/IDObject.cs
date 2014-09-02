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

﻿using BLibrary.Serialization;
using System;
using System.Runtime.Serialization;
using BLibrary;
using BLibrary.Util;
using BLibrary.Network;

namespace Starliners.Game {

    public enum ContentType : byte {
        None,
        Player,
        Asset,
        State,
        Entity
    }

    /// <summary>
    /// Both entities and assets are IDObjects. Entities can be added and removed from the game, assets are static.
    /// </summary>
    [Serializable]
    public abstract class IdObject : SerializableObject, IIdIdentifiable, IUpdateIndicator {

        #region Properties

        public abstract ContentType ContentType { get; }

        /// <summary>
        /// Gets a name for this object.
        /// </summary>
        [GameData (Remote = true, Key = "Name")]
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last DateTime tick on which this idobject was marked as updated.
        /// </summary>
        /// <value>The last update.</value>
        public virtual long LastUpdated {
            get;
            private set;
        }

        #endregion

        #region Constructor

        protected IdObject (IWorldAccess access, string name)
            : this (access, name, access.GetNextSerial ()) {
        }

        protected IdObject (IWorldAccess access, string name, ulong uid)
            : base (access, uid) {
            Name = name;
        }

        #endregion

        #region Serialization

        public IdObject (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        #region Ticking

        /// <summary>
        /// Indicates whether this object needs to be ticked by the simulator.
        /// </summary>
        [GameData (Remote = true, Key = "IsTickable")]
        public bool IsTickable {
            get;
            protected set;
        }

        /// <summary>
        /// Tick the object for the specified tick types.
        /// </summary>
        /// <param name='tickFor'>
        /// List of tick types to tick for.
        /// </param>
        public virtual void Tick (TickType ticks) {
        }

        /// <summary>
        /// Mock a game tick to predict some stuff client side (movement).
        /// </summary>
        public virtual void RenderTick () {
        }

        #endregion

        #region Networking

        Timer _networkUpdateTimer;

        /// <summary>
        /// If set an update will be sent on the next tick, regardless of the network update timer for this entity.
        /// </summary>
        protected bool ForceNetUpdate { get; set; }

        protected UpdateMarker UpdateSchedule { get; private set; }

        /// <summary>
        /// Indicates whether this entity needs to be updated this world tick.
        /// </summary>
        public bool NeedsUpdate {
            get {
                if (CanSendNetworkUpdate ()) {
                    return UpdateSchedule > 0;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates whether this entity has pending net updates.
        /// </summary>
        public bool HasPendingUpdates {
            get {
                return UpdateSchedule > 0;
            }
        }

        /// <summary>
        /// Schedules the given update without triggering the update marker.
        /// </summary>
        /// <param name="marker">Marker.</param>
        protected void ScheduleUpdate (UpdateMarker marker) {
            UpdateSchedule |= marker;
        }

        /// <summary>
        /// Schedules the given update and marks the object as having been updated.
        /// </summary>
        /// <param name="update"></param>
        protected void MarkUpdated (UpdateMarker marker) {
            ScheduleUpdate (marker);
            MarkUpdated ();
        }

        /// <summary>
        /// Marks the object as having been updated.
        /// </summary>
        protected void MarkUpdated () {
            LastUpdated = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Unschedules the given update.
        /// </summary>
        /// <param name="update"></param>
        protected void UnscheduleUpdate (UpdateMarker update) {
            UpdateSchedule &= ~update;
        }

        bool CanSendNetworkUpdate () {
            if (ForceNetUpdate) {
                ForceNetUpdate = false;
                return true;
            }

            if (_networkUpdateTimer == null) {
                _networkUpdateTimer = new Timer (Constants.TICKS_HEARTBEAT * 3);
            }
            if (_networkUpdateTimer.IsDelayed) {
                return false;
            }

            _networkUpdateTimer.Reset ();
            return true;
        }

        /// <summary>
        /// Handles the passed network update.
        /// </summary>
        /// <param name="packet">Packet.</param>
        public virtual void HandleNetworkUpdate (IPacketMarked packet) {
        }

        /// <summary>
        /// Retrieves a network update packet.
        /// </summary>
        /// <returns>The update packet. Null if no update.</returns>
        /// <param name="player">Player.</param>
        public virtual IPacketMarked GetUpdatePacket () {
            return null;
        }

        #endregion
    }
}
