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

namespace Starliners.Game {

    [Serializable]
    public abstract class PlayerTarget : SerializableObject {
        #region Properties

        public abstract Vect2d Location { get; }

        [GameData (Remote = true, Persists = false)]
        public long StartTime {
            get;
            private set;
        }

        [GameData (Remote = true, Persists = false)]
        public int EstimatedDuration {
            get;
            private set;
        }

        #endregion

        #region Constructor

        public PlayerTarget (IWorldAccess access, int activationEstimate)
            : base (access, (ulong)access.Rand.Next ()) {
            StartTime = access.Clock.Ticks;
            EstimatedDuration = activationEstimate;
        }

        #endregion

        #region Serialization

        public PlayerTarget (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public abstract bool OnTargetingTick (Player player, int duration, ControlState control);

        public abstract bool IsTargeted (object obj);
    }

    [Serializable]
    sealed class PlayerTargetEntity : PlayerTarget {

        public override Vect2d Location {
            get {
                return _entity.Location;
            }
        }

        [GameData (Remote = true, Persists = false)]
        Entity _entity;

        #region Constructor

        public PlayerTargetEntity (Player player, Entity entity)
            : base (entity.Access, entity.Blueprint.Interaction.GetEstimatedActivation (entity, player)) {
            _entity = entity;
        }

        #endregion

        #region Serialization

        public PlayerTargetEntity (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override bool OnTargetingTick (Player player, int duration, ControlState control) {
            return _entity.OnActivationTick (player, duration, control);
        }

        public override bool IsTargeted (object obj) {
            return _entity == obj;
        }

    }

}

