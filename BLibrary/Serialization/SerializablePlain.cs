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
    /// <summary>
    /// Serializes any value which may be duplicated. Will not attempt to resolve cyclic dependencies.
    /// </summary>
    sealed class SerializablePlain : SerializableMember {

        public SerializablePlain (MemberWrapper wrapper)
            : base (wrapper.BaseKey, wrapper) {
        }

        public override void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (NeedsDebug) {
                access.Log ("Serialization", "Serializing member {0} ({1}) in type {2} using default serialization.", Key, Wrapper.MemberType, obj.GetType ());
            }

            info.AddValue (Key, Wrapper.GetValue (obj));
        }

        public override void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            Wrapper.SetValue (obj, info.GetValue (Key, Wrapper.MemberType));
        }
    }

}
