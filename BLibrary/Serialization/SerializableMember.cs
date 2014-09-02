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

﻿using System.Runtime.Serialization;
using BLibrary;

namespace BLibrary.Serialization {

    abstract class SerializableMember {

        public bool IsRemote {
            get {
                return Wrapper.GameData.Remote;
            }
        }

        public bool IsPersistent {
            get {
                return Wrapper.GameData.Persists;
            }
        }

        public bool NeedsDebug {
            get {
                return Wrapper.GameData.Debug;
            }
        }

        public bool IsNullable {
            get {
                return Wrapper.GameData.Nullable;
            }
        }

        protected MemberWrapper Wrapper {
            get;
            private set;
        }

        public string Key {
            get;
            private set;
        }

        public SerializableMember (string key, MemberWrapper wrapper) {
            Key = key;
            Wrapper = wrapper;
        }

        public abstract void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context);

        public abstract void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context);

        public virtual void OnDeserialized (IIdObjectAccess access, ISerializedLinked obj) {
        }

    }

}
