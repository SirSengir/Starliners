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
using System.Linq;
using System.Runtime.Serialization;
using BLibrary;


namespace BLibrary.Serialization {

    /// <summary>
    /// Serializes a IList of IDObjects.
    /// </summary>
    sealed class SerializableIdList : SerializableMember {
        public SerializableIdList (MemberWrapper wrapper)
            : base (wrapper.BaseKey, wrapper) {
        }

        public override void Serialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            Type elementtype = Wrapper.MemberType.GetGenericArguments () [0];
            if (NeedsDebug) {
                access.Log ("Serialization", "Serializing member {0} ({1}) in type {2} as an IDAssetList.", Key, elementtype, obj.GetType ());
            }

            ICollection<IIdIdentifiable> list = ((System.Collections.ICollection)Wrapper.GetValue (obj)).Cast<IIdIdentifiable> ().ToList ();
            IList<ulong> idlist = list.Select (p => p.Serial).Cast<ulong> ().ToList ();
            info.AddValue (Key, idlist);
        }

        public override void Deserialize (IIdObjectAccess access, ISerializedLinked obj, SerializationInfo info, StreamingContext context) {
            if (obj.CacheSerializables == null) {
                obj.CacheSerializables = new SerialCache ();
            }

            obj.CacheSerializables [Key] = info.GetValue (Key, typeof(IList<ulong>));
        }

        public override void OnDeserialized (IIdObjectAccess access, ISerializedLinked obj) {
            base.OnDeserialized (access, obj);
            if (NeedsDebug) {
                access.Log ("Serialization", "Recreating IDObject list for field {0} ({1}) in type {2}.", Key, Wrapper.MemberType, obj.GetType ());
            }

            IList<ulong> idlist = (IList<ulong>)obj.CacheSerializables [Key];
            foreach (ulong uid in idlist) {
                IIdIdentifiable idobject = access.RequireIDObject (uid);
                ((System.Collections.IList)Wrapper.GetValue (obj)).Add (idobject);
                idobject.OnDeserialization (this);
            }
        }
    }
}
