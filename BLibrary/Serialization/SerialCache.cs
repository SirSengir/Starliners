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

namespace BLibrary.Serialization {

    /// <summary>
    /// A simple wrapper for a Dictionary to hold arbritary values which can be serials, lists of serials or Key->Serial pairs.
    /// </summary>
    public sealed class SerialCache {
        #region Index

        public object this [string key] {
            get {
                return _cached [key];
            }
            set {
                _cached [key] = value;
            }
        }

        #endregion

        Dictionary<string, object> _cached = new Dictionary<string, object> ();

        #region Constructor

        public SerialCache () {
        }

        #endregion

        public bool HasKey (string key) {
            return _cached.ContainsKey (key);
        }

        public void Clear () {
            _cached.Clear ();
        }
    }
}

