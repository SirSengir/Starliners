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
using System.Linq;

namespace BLibrary.Util {

    public sealed class Counter<T> {

        /// <summary>
        /// Gets the most commonly encountered object.
        /// </summary>
        /// <value>The most common.</value>
        public T MostFrequent {
            get {
                KeyValuePair<T, int> entry = _counts.OrderByDescending (p => p.Value).FirstOrDefault ();
                return entry.Key;
            }
        }

        Dictionary<T, int> _counts = new Dictionary<T, int> ();

        /// <summary>
        /// Increases the counter for the given object. Ignores null.
        /// </summary>
        /// <param name="counted">Counted.</param>
        public void Count (T counted) {
            if (counted == null) {
                return;
            }

            if (_counts.ContainsKey (counted)) {
                _counts [counted]++;
            } else {
                _counts [counted] = 1;
            }
        }

    }
}

