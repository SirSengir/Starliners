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
using BLibrary;

namespace BLibrary.Serialization {

    /// <summary>
    /// Serializes a plain IdObject.
    /// </summary>
    sealed class SerializableIdObject : SerializableMember {
        public SerializableIdObject (MemberWrapper wrapper)
            : base (wrapper.BaseKey, wrapper) {
        }

        public override void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (NeedsDebug) {
                access.Log ("Serialization", "Serializing member {0} ({1}) in type {2} as IIDIdentifiable.", Key, Wrapper.MemberType, obj.GetType ());
            }

            IIdIdentifiable memberValue = (IIdIdentifiable)Wrapper.GetValue (obj);
            if (memberValue != null) {
                info.AddValue (Key, memberValue.Serial);
            } else if (IsNullable) {
                info.AddValue (Key, LibraryConstants.NULL_ID);
            } else {
                throw new SystemException (string.Format ("Unable to serialize '{0}' ({1}) in type {2} as null since it may not be null.", Key, Wrapper.MemberType, obj.GetType ()));
            }
        }

        public override void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (obj.CacheSerializables == null) {
                obj.CacheSerializables = new SerialCache ();
            }
            obj.CacheSerializables [Key] = info.GetUInt64 (Key);
        }

        public override void OnDeserialized (IIdObjectAccess access, ISerializedLinked obj) {
            base.OnDeserialized (access, obj);
            if (NeedsDebug) {
                access.Log ("Serialization", "Recreating IDObject for field {0} ({1}) in type {2} from id {3}.", Key, Wrapper.MemberType, obj.GetType (), obj.CacheSerializables [Key]);
            }

            ulong uid = (ulong)obj.CacheSerializables [Key];
            if (uid != LibraryConstants.NULL_ID) {
                IIdIdentifiable idobject = access.RequireIDObject (uid);
                Wrapper.SetValue (obj, idobject);
                idobject.OnDeserialization (this);
            } else if (IsNullable) {
                Wrapper.SetValue (obj, null);
            } else {
                throw new SystemException (string.Format ("Unable to deserialize '{0}' ({1}) in type {2} as null since it may not be null.", Key, Wrapper.MemberType, obj.GetType ()));
            }
        }
    }
}
