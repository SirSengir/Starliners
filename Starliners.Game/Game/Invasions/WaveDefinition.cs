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
using Starliners.Game.Forces;
using System.Collections.Generic;
using BLibrary.Json;

namespace Starliners.Game.Invasions {
    sealed class WaveDefinition {

        public int Id {
            get;
            private set;
        }

        int _first;
        int _last;
        float _padding;

        Dictionary<ShipSize, int> _maintenance = new Dictionary<ShipSize, int> ();
        Dictionary<string, int> _attributes = new Dictionary<string, int> ();

        public WaveDefinition (JsonObject json) {
            Id = (int)json ["id"].GetValue<double> ();
            _first = json.ContainsKey ("first") ? (int)json ["first"].GetValue<double> () : 0;
            _last = json.ContainsKey ("last") ? (int)json ["last"].GetValue<double> () : -1;
            _padding = json.ContainsKey ("padding") ? (float)json ["padding"].GetValue<double> () : 0;

            foreach (var entry in json["maintenance"].GetValue<JsonObject>()) {
                _maintenance [(ShipSize)Enum.Parse (typeof(ShipSize), entry.Key, true)] = (int)entry.Value.GetValue<double> ();
            }
            if (json.ContainsKey ("attributes")) {
                foreach (var entry in json["attributes"].GetValue<JsonObject>()) {
                    _attributes [entry.Key] = (int)entry.Value.GetValue<double> ();
                }
            }

        }

        public int GetMaintenance (ShipSize size, int waveCount) {
            int maintenance = _maintenance.ContainsKey (size) ? _maintenance [size] : 0;
            return maintenance + (int)(maintenance * _padding * (waveCount - _first));
        }

        public ShipModifiers CreateShipModifiers (ShipSize size) {
            return new ShipModifiers ();
        }

        public int GetAttribute (string key) {
            return _attributes.ContainsKey (key) ? _attributes [key] : 0;
        }

        /// <summary>
        /// Determines whether this wave definition is active at the specified waveCount.
        /// </summary>
        /// <returns><c>true</c> if this instance is active the specified waveCount; otherwise, <c>false</c>.</returns>
        /// <param name="waveCount">Wave count.</param>
        public bool IsActive (int waveCount) {
            if (waveCount < _first) {
                return false;
            }
            if (_last > 0 && waveCount > _last) {
                return false;
            }
            return true;
        }
    }
}

