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

namespace BLibrary.Util {

    /// <summary>
    /// A simple class to hold some arbitrary data. Serializable.
    /// </summary>
    [Serializable]
    public sealed class MetaContainer : ISerializable {

        Dictionary<string, object> _parameters = new Dictionary<string, object> ();

        public MetaContainer () {
        }

        #region Serialization

        public MetaContainer (SerializationInfo info, StreamingContext context) {
            int count = info.GetInt32 ("PCount");
            for (int i = 0; i < count; i++) {
                string key = info.GetString ("PKey." + i);
                _parameters [key] = info.GetValue ("PVal." + i, typeof(object));
            }
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("PCount", _parameters.Count);

            int count = 0;
            foreach (KeyValuePair<string, object> entry in _parameters) {
                info.AddValue ("PKey." + count, entry.Key);
                info.AddValue ("PVal." + count, entry.Value);
                count++;
            }
        }

        #endregion

        public T Get<T> (string key) {
            return (T)_parameters [key];
        }

        public void Set (string key, object value) {
            _parameters [key] = value;
        }

        public string[] GetInfo () {
            List<string> info = new List<string> ();
            foreach (KeyValuePair<string, object> entry in _parameters) {
                info.Add (string.Format ("{0} => {1}", entry.Key, entry.Value));
            }
            return info.ToArray ();
        }
    }
}

