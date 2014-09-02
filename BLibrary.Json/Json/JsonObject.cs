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

namespace BLibrary.Json {

    public sealed class JsonObject : IReadOnlyDictionary<string, JsonNode> {

        #region Properties

        public JsonNode this [string index] {
            get {
                return _dictionary [index];
            }
        }

        public IEnumerable<string> Keys {
            get {
                return _dictionary.Keys;
            }
        }

        public IEnumerable<JsonNode> Values {
            get {
                return _dictionary.Values;
            }
        }

        public int Count {
            get {
                return _dictionary.Count;
            }
        }

        #endregion

        IReadOnlyDictionary<string, JsonNode> _dictionary;

        public JsonObject (IReadOnlyDictionary<string, JsonNode> dictionary) {
            _dictionary = dictionary;
        }

        public bool ContainsKey (string key) {
            return _dictionary.ContainsKey (key);
        }

        public bool TryGetValue (string key, out JsonNode value) {
            return _dictionary.TryGetValue (key, out value);
        }

        public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator () {
            return _dictionary.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return _dictionary.GetEnumerator ();
        }
    }
}

