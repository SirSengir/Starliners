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
using System.Runtime.Serialization;

namespace BLibrary.Util {

    /// <summary>
    /// Since a normal dictionary throws an NPE even in OnDeserializationComplete, this implements custom serialization.
    /// </summary>
    [Serializable]
    public sealed class DictionaryWrapper<T, K> : ISerializable {

        public K this[T key] {
            get {
                return _wrapped [key];
            }
            set {
                _wrapped [key] = value;
            }
        }

        public Dictionary<T, K>.KeyCollection Keys {
            get {
                return _wrapped.Keys;
            }
        }

        Dictionary<T, K> _wrapped;

        #region Constructor
        public DictionaryWrapper (Dictionary<T, K> wrapped) {
            _wrapped = wrapped;
        }
        #endregion

        #region Serialization
        public DictionaryWrapper (SerializationInfo info, StreamingContext context) {
            _wrapped = new Dictionary<T, K> ();

            for (int i = 0; i < info.GetInt32 ("EntryCount"); i++) {
                T key = (T)info.GetValue (string.Format ("EntryKey.{0}", i), typeof(T));
                K value = (K)info.GetValue (string.Format ("EntryValue.{0}", i), typeof(K));
                _wrapped [key] = value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue ("EntryCount", _wrapped.Count);

            int count = 0;
            foreach (var entry in _wrapped) {
                info.AddValue (string.Format ("EntryKey.{0}", count), entry.Key, typeof(T));
                info.AddValue (string.Format ("EntryValue.{0}", count), entry.Value, typeof(K));
                count++;
            }
        }
        #endregion

        public bool ContainsKey(T key) {
            return _wrapped.ContainsKey (key);
        }
    }
}

