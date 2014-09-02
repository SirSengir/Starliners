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
using BLibrary.Util;


namespace BLibrary.Serialization {

    sealed class SerializableIdDictValue : SerializableMember {
        public SerializableIdDictValue (MemberWrapper wrapper)
            : base (wrapper.BaseKey, wrapper) {
        }

        public override void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (NeedsDebug) {
                Type elementtype = Wrapper.MemberType.GetGenericArguments () [1];
                access.Log ("Serialization", "Serializing member {0} ({1}) in type {2} as a dictionary of IDObjects.", Key, elementtype, obj.GetType ());
            }

            System.Collections.IDictionary list = ((System.Collections.IDictionary)Wrapper.GetValue (obj));

            StringUlongPair[] enumerable = new StringUlongPair[list.Count];
            int count = 0;
            foreach (string key in list.Keys) {
                enumerable [count] = new StringUlongPair (key, ((IIdIdentifiable)list [key]).Serial);
                count++;
            }
            info.AddValue (Key, enumerable, typeof(StringUlongPair[])); 
        }

        public override void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (obj.CacheSerializables == null) {
                obj.CacheSerializables = new SerialCache ();
            }

            StringUlongPair[] enumerable = info.GetValue (Key, typeof(StringUlongPair[])) as StringUlongPair[];
            obj.CacheSerializables [Key] = enumerable;
        }

        public override void OnDeserialized (IIdObjectAccess access, ISerializedLinked obj) {
            base.OnDeserialized (access, obj);
            if (NeedsDebug) {
                //access.GameConsole.Serialization ("Recreating IDObject dictionary for field {0} ({1}) in type {2}.", Key, Wrapper.MemberType, obj.GetType ());
            }

            foreach (StringUlongPair entry in (StringUlongPair[])obj.CacheSerializables[Key]) {
                if (NeedsDebug) {
                    //access.GameConsole.Serialization ("{0} adding {1}->{2}.", Key, entry.Key, entry.Value);
                }
                IIdIdentifiable idobject = access.RequireIDObject (entry.Value);
                ((System.Collections.IDictionary)Wrapper.GetValue (obj)).Add (entry.Key, idobject);
                idobject.OnDeserialization (this);
            }
        }
    }
}
