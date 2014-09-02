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
using BLibrary.Serialization;
using System.Runtime.Serialization;

namespace BLibrary.Util {

    [Serializable]
    public abstract class DataFragment {
        #region Properties

        [GameData (Remote = true)]
        public string Key { get; protected set; }

        public bool IsDirty { get; set; }

        #endregion

        public DataFragment () {
            IsDirty = true;
        }

    }

    [Serializable]
    public sealed class DataFragment<T> : DataFragment, ISerializedLinked, IDeserializationCallback {

        #region Properties

        public SerialCache CacheSerializables {
            get;
            set;
        }

        [GameData (Remote = true)]
        public T Value {
            get;
            set;
        }

        #endregion

        IIdObjectAccess _access;

        public DataFragment (string key, T value) {
            Key = key;
            Value = value;
        }

        #region Serialization

        public DataFragment (SerializationInfo info, StreamingContext context) {
            _access = (IIdObjectAccess)context.Context;
            _access.SerializationHelper.Deserialize (this, info, context);
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            _access = (IIdObjectAccess)context.Context;
            _access.SerializationHelper.Serialize (this, info, context);
        }

        public void OnDeserialization (object sender) {
            _access.SerializationHelper.OnDeserialized (this);
        }

        public void MakeSane () {
        }

        #endregion

    }

}

