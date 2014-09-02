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
using BLibrary.Serialization;

namespace Starliners.Game {
    /// <summary>
    /// A list able to contain and correctly serialize IdObjects.
    /// </summary>
    [Serializable]
    public sealed class IdList<T> : SerializableObject, IEnumerable<T> where T : SerializableObject {

        public T this [int index] {
            get {
                return _items [index];
            }
        }

        public IReadOnlyList<T> Listing {
            get {
                return _items;
            }
        }

        [GameData (Remote = true, Key = "InnerList")]
        List<T> _items = new List<T> ();

        public IdList (IWorldAccess access)
            : base (access) {
        }

        #region Serialization

        public IdList (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public void Add (T item) {
            _items.Add (item);
        }

        public void Remove (T item) {
            _items.Remove (item);
        }

        public IEnumerator<T> GetEnumerator () {
            return _items.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return _items.GetEnumerator ();
        }
    }
}

