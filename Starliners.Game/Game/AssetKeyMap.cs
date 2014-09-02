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
using BLibrary.Util;


namespace Starliners.Game {

    [Serializable]
    public sealed class AssetKeyMap : ISerializable {
        Dictionary<string, ulong> _keyToUidMap = new Dictionary<string, ulong> ();

        public ulong this [IWorldAccess access, string key] {
            get {
                return MapUID (access, key);
            }
        }

        public AssetKeyMap () {
        }

        #region Serialization

        public AssetKeyMap (SerializationInfo info, StreamingContext context) {
            StringUlongPair[] enumerable = info.GetValue ("KeyToUIDMap", typeof(StringUlongPair[])) as StringUlongPair[];
            foreach (StringUlongPair entry in enumerable) {
                _keyToUidMap [entry.Key] = entry.Value;
            }
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            StringUlongPair[] enumerable = _keyToUidMap.Select (p => new StringUlongPair (p.Key, p.Value)).ToArray ();
            info.AddValue ("KeyToUIDMap", enumerable, typeof(StringUlongPair[]));
        }

        #endregion

        ulong MapUID (IWorldAccess access, string key) {
            if (access == null)
                throw new ArgumentNullException ("access");
            if (key == null)
                throw new ArgumentNullException ("key");

            if (!_keyToUidMap.ContainsKey (key))
                _keyToUidMap.Add (key, access.GetNextSerial ());

            return _keyToUidMap [key];
        }

        public T RetrieveAsset<T> (IWorldAccess access, string key) where T : Asset {
            if (access == null)
                throw new ArgumentNullException ("access");
            if (key == null)
                throw new ArgumentNullException ("key");
            if (!_keyToUidMap.ContainsKey (key))
                throw new ArgumentException ("Key cannot be mapped: " + key);

            return access.RequireAsset<T> (_keyToUidMap [key]);
        }
    }
}
