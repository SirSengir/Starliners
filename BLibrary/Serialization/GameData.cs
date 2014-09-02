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

namespace BLibrary.Serialization {

    /// <summary>
    /// Attribute to tag fields and properties for serialization. Properties need to have getter and setter.
    /// </summary>
    public sealed class GameData : Attribute {

        /// <summary>
        /// Gets or sets a value indicating whether the field or property marked with this <see cref="BSimulator.Serialization.GameData"/> will be serialized for network synch.
        /// </summary>
        /// <value>
        /// <c>true</c> if remoting; otherwise, <c>false</c>.
        /// </value>
        public bool Remote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BSimulator.Serialization.GameData"/> should be persisted into savegames.
        /// </summary>
        /// <value><c>true</c> if persist; otherwise, <c>false</c>.</value>
        public bool Persists { get; set; }

        /// <summary>
        /// Enables additional debug output in the log.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Will throw an exception if serialization/deserialization to null is attempted.
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets the key used to identify this attribute. If not set, the field or property name will be used.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        public GameData () {
            Persists = true;
            Nullable = true;
        }

        /// <summary>
        /// Returns the key used to identify this attribute.
        /// </summary>
        /// <returns>
        /// The key.
        /// </returns>
        /// <param name='fieldName'>
        /// Field name.
        /// </param>
        public string GetKey (string fieldName) {
            if (Key != string.Empty && Key != null)
                return Key;
            return fieldName;
        }
    }
}
