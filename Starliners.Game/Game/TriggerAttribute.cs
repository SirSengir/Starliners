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
using BLibrary.Json;
using Starliners.Game.Planets;

namespace Starliners.Game {
    [Serializable]
    sealed class TriggerAttribute : Trigger {

        string _attribute;
        int _minimum;
        int _maximum;

        #region Constructor

        public TriggerAttribute (IWorldAccess access, IPopulator populator, JsonObject json)
            : base (access) {
            _attribute = json.ContainsKey ("value") ? json ["value"].GetValue<string> () : string.Empty;
            _minimum = json.ContainsKey ("minimum") ? (int)json ["minimum"].GetValue<double> () : -1;
            _maximum = json.ContainsKey ("maximum") ? (int)json ["maximum"].GetValue<double> () : -1;
        }

        #endregion

        #region Serialization

        public TriggerAttribute (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Determines whether this instance is tripped by the specified planet.
        /// </summary>
        /// <returns><c>true</c> if this instance is tripped the specified planet; otherwise, <c>false</c>.</returns>
        /// <param name="planet">Planet.</param>
        public override bool IsTripped (ILevyProvider provider) {
            Planet planet = provider as Planet;
            return planet == null ? false : IsTripped (planet);
        }

        public override bool IsTripped (Planet planet) {
            int value = planet.Attributes [_attribute];
            if (_minimum >= 0 && value < _minimum) {
                return false;
            }
            if (_maximum >= 0 && value > _maximum) {
                return false;
            }
            return true;
        }

    }
}
