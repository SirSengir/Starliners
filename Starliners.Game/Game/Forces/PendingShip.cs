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

namespace Starliners.Game.Forces {
    [Serializable]
    sealed class PendingShip : ISerializable {

        public readonly ulong Levy;
        public readonly ulong Ship;

        public PendingShip (Levy levy, ShipInstance ship) {
            Levy = levy.Serial;
            Ship = ship.Serial;
        }

        public PendingShip (SerializationInfo info, StreamingContext context) {
            Levy = info.GetUInt64 ("Levy");
            Ship = info.GetUInt64 ("Ship");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Levy", Levy);
            info.AddValue ("Ship", Ship);
        }
    }
}

