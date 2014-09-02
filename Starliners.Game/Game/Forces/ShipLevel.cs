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

namespace Starliners.Game.Forces {
    public enum ShipLevel : byte {
        None,
        Militia,
        Regular,
        Veteran,
        Elite
    }

    public static class ShipLevels {
        public static int GetTracking (ShipLevel level, int tracking) {
            switch (level) {
                case ShipLevel.Elite:
                    return (int)(tracking * 1.4);
                case ShipLevel.Veteran:
                    return (int)(tracking * 1.2);
                case ShipLevel.Regular:
                    return tracking;
                default:
                    return (int)(tracking * 0.8);
            }
        }

        public static int GetManouver (ShipLevel level, int manouver) {
            switch (level) {
                case ShipLevel.Elite:
                    return (int)(manouver * 1.4);
                case ShipLevel.Veteran:
                    return (int)(manouver * 1.2);
                case ShipLevel.Regular:
                    return manouver;
                default:
                    return (int)(manouver * 0.8);
            }
        }
    }
}

