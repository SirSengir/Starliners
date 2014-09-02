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

namespace Starliners.Game {

    /// <summary>
    /// Basic class which everything that needs to synch between server and client state needs to inherit from.
    /// </summary>
    [Serializable]
    public class SerializableObject : ISerializedLinked, IDeserializationCallback {
        #region Properties

        /// <summary>
        /// Gets a unique serial for this object.
        /// </summary>
        /// <remarks>
        /// Uniqueness is only guaranteed within a type of objects.
        /// IDObjects have unique serials to other IDObjects, ItemStacks to other ItemStacks.
        /// </remarks>
        /// <value>The serial.</value>
        public ulong Serial { get { return _serial; } }

        public SerialCache CacheSerializables { get; set; }

        /// <summary>
        /// Gets or sets the world access this object is associated with.
        /// </summary>
        /// <value>The world access.</value>
        public IWorldAccess Access { get; protected set; }

        #endregion

        #region Fields

        [GameData (Remote = true, Key = "Serial")]
        readonly ulong _serial;
        bool _isFullyDeserialized;
        bool _isSane;

        #endregion

        #region Constructor

        protected SerializableObject (IWorldAccess access)
            : this (access, access.GetNextSerial ()) {
        }

        protected SerializableObject (IWorldAccess access, ulong serial) {
            Access = access;
            _serial = serial;
        }

        public void MakeSane () {
            if (!_isSane) {
                _isSane = true;
                OnCommissioned ();
            }
        }

        protected virtual void OnCommissioned () {
        }

        #endregion

        #region Serialization

        public SerializableObject (SerializationInfo info, StreamingContext context) {
            Access = (IWorldAccess)context.Context;
            Access.SerializationHelper.Deserialize (this, info, context);
        }

        public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
            Access = (IWorldAccess)context.Context;
            Access.SerializationHelper.Serialize (this, info, context);
        }

        public void OnDeserialization (object sender) {
            if (!_isFullyDeserialized) {
                // Must be set first, otherwise cycle references blow up!
                _isFullyDeserialized = true;
                Access.SerializationHelper.OnDeserialized (this);
            }
        }

        #endregion
    }
}
