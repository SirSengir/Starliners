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
using System.Collections.Generic;

namespace Starliners.Game.Planets {

    public enum PlanetType {
        None,
        Aquatic,
        Arid,
        Barren,
        Gaseous,
        Primordial,
        Swamp,
        Terran
    }

    public static class PlanetTypes {
        public static readonly PlanetType[] VALUES = (PlanetType[])Enum.GetValues (typeof(PlanetType));
        public static readonly IReadOnlyDictionary<PlanetType, int> SKINS = new Dictionary<PlanetType, int> {
            { PlanetType.Barren, 1 },
            { PlanetType.Primordial, 1 },
            { PlanetType.Arid, 6 },
            { PlanetType.Swamp, 4 },
            { PlanetType.Aquatic, 4 },
            { PlanetType.Terran, 4 },
            { PlanetType.Gaseous, 2 },
        };
    }
}

