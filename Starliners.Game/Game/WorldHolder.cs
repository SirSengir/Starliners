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
using BLibrary;
using BLibrary.Util;
using BLibrary.Serialization;
using Starliners.Game.Forces;

namespace Starliners.Game {

    public abstract class WorldHolder {
        #region Properties

        public IWorldEditor Access {
            get;
            protected set;
        }

        public abstract bool IsSimulating {
            get;
        }

        public abstract GameConsole GameConsole {
            get;
        }

        #endregion

        protected void HandleInsanity () {
            while (Access.SerializationHelper.Added.Count > 0) {
                ISerializedLinked added = null;
                if (Access.SerializationHelper.Added.TryDequeue (out added)) {
                    added.MakeSane ();
                }
            }
        }

        public virtual void SendChat (Player player, TextComposition chat) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #region Players

        public virtual void SetView (Player player, Vect2f center) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #endregion

        #region Map

        /// <summary>
        /// Use to signal a map or entity change which can affect entity paths.
        /// </summary>
        public virtual void SignalPathsChanged () {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        /// <summary>
        /// Use to signal a map or entity change which requires a re-render of the static parts of the map.
        /// </summary>
        public virtual void SignalMapDirtied (Vect2d location, string reason) {
            throw new SystemException ("Cannot execute this method in the simulator.");
        }

        #endregion

        #region Entities

        public virtual void QueueEntity (Entity entity) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual void QueueEntityRemoval (Entity entity) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual EntityFleet SpawnFleetEntity (Fleet fleet) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #endregion

        #region States

        public virtual void QueueState (StateObject state) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual void QueueStateRemoval (StateObject state) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #endregion

        /// <summary>
        /// Synchronizes the currently held object to the player.
        /// </summary>
        /// <param name="player"></param>
        public virtual void SynchHeld (Player player) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        /// <summary>
        /// Updates the held object on the given player, synching it if needed.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="holdable"></param>
        public virtual void UpdateHeld (Player player, IHoldable holdable) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #region Particles

        public virtual void SpawnParticle (Particle particle) {
            Access.AddParticle (particle);
        }

        /*
        public virtual void SpawnParticle (string text, Vect2f coordinates) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual void SpawnParticle (string text, string icon, Colour colour, Vect2f coordinates) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }
        */

        #endregion

        #region Sound

        public virtual void PlaySound (string sound, Vect2f coordinates) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual void PlaySound (string sound, Player player, Vect2f coordinates) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        public virtual void PlaySound (string sound, Player player) {
            throw new SystemException ("Cannot execute this method in the interface.");
        }

        #endregion

    }
}
