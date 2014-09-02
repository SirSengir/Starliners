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
using System.Collections.Generic;

namespace BLibrary.Serialization {
    sealed class SerializableHashSet<T> : SerializableMember {
        public SerializableHashSet (MemberWrapper wrapper)
            : base (wrapper.BaseKey, wrapper) {
        }

        public override void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (NeedsDebug) {
                Type elementtype = Wrapper.MemberType.GetGenericArguments () [1];
                access.Log ("Serialization", "Serializing member {0} ({1}) in type {2} as a hashset.", Key, elementtype, obj.GetType ());
            }

            HashSet<T> hashset = (HashSet<T>)Wrapper.GetValue (obj);
            T[] newarr = new T[hashset.Count];
            hashset.CopyTo (newarr);
            info.AddValue (Key, newarr, typeof(T[])); 
        }

        public override void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            T[] stored = info.GetValue (Key, typeof(T[])) as T[];
            HashSet<T> hashset = new HashSet<T> ();
            foreach (T store in stored) {
                hashset.Add (store);
            }
            Wrapper.SetValue (obj, hashset);
        }

    }
}

