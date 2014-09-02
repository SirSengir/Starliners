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
using System.Collections.Generic;
using BLibrary.Util;

namespace Starliners.Game {

    [Serializable]
    public abstract class InteractionHandler {
        #region Constants

        static readonly string[] NO_INFORMATION = new string[0];
        public const int ACTIVATION_DELAY_DEFAULT = 15;

        #endregion

        public abstract int GetEstimatedActivation (Entity entity, Player player);

        /// <summary>
        /// Determines whether the given entity can be activated by the given player and the given controlstate.
        /// </summary>
        /// <returns><c>true</c> if this instance can activate the specified entity player control; otherwise, <c>false</c>.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="player">Player.</param>
        /// <param name="control">Control.</param>
        public abstract bool CanActivate (Entity entity, Player player, ControlState control);

        /// <summary>
        /// Determines whether the given entity was activated by the given player and controls at the given duration.
        /// </summary>
        /// <returns><c>true</c> if this instance is activated the specified entity player control duration; otherwise, <c>false</c>.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="player">Player.</param>
        /// <param name="control">Control.</param>
        /// <param name="duration">Duration.</param>
        public abstract bool IsActivated (Entity entity, Player player, ControlState control, int duration);

        /// <summary>
        /// Raised when (another) player collides with this entity.
        /// </summary>
        /// <param name="player">Player.</param>
        public abstract void OnPlayerCollision (Entity entity, Player player);

        /// <summary>
        /// Raised when the entity was activated.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="player">Player.</param>
        /// <param name="control">Control.</param>
        public abstract void OnActivated (Entity entity, Player player, ControlState control);

        /// <summary>
        /// Raised when the entity was interacted with.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="player">Player.</param>
        /// <param name="control">Control.</param>
        public abstract void OnInteracted (Entity entity, Player player, ControlState control);

        /// <summary>
        /// Gets usage help for the given entity and player.
        /// </summary>
        /// <returns>The usage.</returns>
        /// <param name="entity">Entity.</param>
        /// <param name="player">Player.</param>
        public virtual IList<string> GetUsage (Entity entity, Player player) {
            return NO_INFORMATION;
        }
    }
}

