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
using Starliners.Game.Planets;
using BLibrary.Json;
using System.Runtime.Serialization;

namespace Starliners.Game {
    [Serializable]
    sealed class TriggerBuilding : Trigger {

        Improvement _building;
        string _flag;
        int _minimum;
        int _maximum;

        #region Constructor

        public TriggerBuilding (IWorldAccess access, IPopulator populator, JsonObject json)
            : base (access) {
            AssetHolder<Improvement> buildings = (AssetHolder<Improvement>)populator.Holders [AssetKeys.IMPROVEMENTS];
            _building = json.ContainsKey ("value") ? buildings.GetAsset (json ["value"].GetValue<string> ()) : null;
            _flag = json.ContainsKey ("flag") ? json ["flag"].GetValue<string> () : string.Empty;
            _minimum = json.ContainsKey ("minimum") ? (int)json ["minimum"].GetValue<double> () : 1;
            _maximum = json.ContainsKey ("maximum") ? (int)json ["maximum"].GetValue<double> () : -1;
        }

        #endregion

        #region Serialization

        public TriggerBuilding (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Determines whether this instance is tripped by the specified planet.
        /// </summary>
        /// <returns><c>true</c> if this instance is tripped the specified planet; otherwise, <c>false</c>.</returns>
        /// <param name="planet">Planet.</param>
        public override bool IsTripped (ILevyProvider planet) {
            return false;
        }

        public override bool IsTripped (Planet planet) {
            int amount = 0;
            if (_building != null) {
                if (!planet.Sectors.ContainsKey (_building.Serial)) {
                    return false;
                }
                amount = planet.Sectors [_building.Serial].Amount;
            } else {
                foreach (BuildingSector sector in planet.Sectors.Values) {
                    if (sector.Improvement.Flags.Contains (_flag)) {
                        amount += sector.Amount;
                    }
                }
            }

            if (_minimum >= 0 && amount < _minimum) {
                return false;
            }
            if (_maximum >= 0 && amount > _maximum) {
                return false;
            }

            return true;
        }

    }
}

