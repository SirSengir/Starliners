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
using BLibrary.Json;

namespace Starliners.Game {
    [Serializable]
    public sealed class CostCalculator {

        enum CostProgression {
            None,
            Fixed,
            Linear,
            Exponential
        }

        readonly int _cost;
        readonly CostProgression _progression;

        public CostCalculator (JsonObject json) {
            _cost = (int)json ["value"].GetValue<double> ();
            _progression = json.ContainsKey ("progression") ? (CostProgression)Enum.Parse (typeof(CostProgression), json ["progression"].GetValue<string> (), true) : CostProgression.Fixed;
        }

        public CostCalculator (int cost) {
            _cost = cost;
            _progression = CostProgression.Fixed;
        }

        public int DeterminePurchasePrice (int owned) {
            switch (_progression) {
                case CostProgression.Exponential:
                    return (1 + owned * owned) * _cost;
                case CostProgression.Linear:
                    return (1 + owned) * _cost;
                default:
                    return _cost;
            }
        }

        public int DetermineSellPrice () {
            return _progression == CostProgression.Fixed ? _cost / 2 : _cost;
        }
    }
}

