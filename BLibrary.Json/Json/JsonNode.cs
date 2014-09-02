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

    public abstract class JsonNode {

        public JsonValueType Type {
            get;
            private set;
        }

        object _value;

        public JsonNode (JsonValueType type, object value) {
            Type = type;
            _value = value;
        }

        public T GetValue<T> () {
            return (T)_value;
        }

        /// <summary>
        /// Convenience function which assumes this node to be a JsonArray and returns the array as an enumerable of the given type.
        /// </summary>
        /// <returns>The enumerable.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IEnumerable<T> AsEnumerable<T> () {
            return ((JsonArray)_value).GetEnumerable<T> ();
        }
    }

    public sealed class JsonNode<T> : JsonNode {

        public T Value {
            get;
            private set;
        }

        public JsonNode (JsonValueType type, T value)
            : base (type, value) {
            Value = value;
        }

    }
}

