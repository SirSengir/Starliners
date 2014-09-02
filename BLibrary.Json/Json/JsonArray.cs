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

﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace BLibrary.Json {

    public sealed class JsonArray : IReadOnlyList<JsonNode> {
        #region Classes

        sealed class JsonArrayEnumerator<T> : IEnumerator<T> {
            int _current = -1;
            JsonArray _target;

            public JsonArrayEnumerator (JsonArray targetList) {
                _target = targetList;
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            public T Current {
                get { return _target [_current].GetValue<T> (); }
            }

            public bool MoveNext () {
                if (_current < 0) {
                    _current = 0;
                } else {
                    _current++;
                }
                return _current < _target.Count;
            }

            public void Reset () {
                _current = -1;
            }

            public void Dispose () {

            }
        }

        #endregion

        #region Properties

        public JsonNode this [int index] {
            get {
                return _list [index];
            }
        }

        public int Count {
            get {
                return _list.Count;
            }
        }

        #endregion

        IReadOnlyList<JsonNode> _list;

        public JsonArray (IReadOnlyList<JsonNode> list) {
            _list = list;
        }

        public IEnumerator<JsonNode> GetEnumerator () {
            return _list.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return _list.GetEnumerator ();
        }

        public IEnumerable<T> GetEnumerable<T> () {
            return _list.Select (p => p.GetValue<T> ());
        }
    }
}

