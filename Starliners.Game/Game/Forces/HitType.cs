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
    [Flags]
    public enum HitType {
        None = 0,
        Missed = 1 << 0,
        Grazed = 1 << 1,
        Notable = 1 << 2,
        Direct = 1 << 3,
        Critical = 1 << 4,
        Final = 1 << 5
    }

    public static class HitTypes {
        public static readonly HitType[] VALID_HITS = new HitType[] {
            HitType.Missed, HitType.Grazed, HitType.Notable, HitType.Direct, HitType.Critical
        };

        public static int GetRandomizedDamage (Random rand, HitType type, int damage) {
            double modifier;
            switch (type) {
                case HitType.Critical:
                    modifier = 1.5;
                    break;
                case HitType.Direct:
                    modifier = 1.25;
                    break;
                case HitType.Notable:
                    modifier = 1.0;
                    break;
                case HitType.Grazed:
                    modifier = 0.75;
                    break;
                default:
                    modifier = 0;
                    break;
            }

            double max = damage * modifier;
            return (int)(max / 2 + rand.NextDouble () * max / 2);
        }
    }
}

