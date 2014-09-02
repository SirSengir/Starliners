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

namespace Starliners.Game {

    [Serializable]
    public sealed class GameClock {
        #region Constants

        public const int TICKS_PER_PARTIAL = 50;
        const int PARTIALS_PER_ROTATION = 20;
        public const int TICKS_PER_ROTATION = TICKS_PER_PARTIAL * PARTIALS_PER_ROTATION;
        const int ROTATIONS_PER_CYCLE = 10;
        public const int TICKS_PER_CYCLE = TICKS_PER_ROTATION * ROTATIONS_PER_CYCLE;
        const int CYCLES_PER_ORBIT = 50;
        public const int TICKS_PER_ORBIT = TICKS_PER_CYCLE * CYCLES_PER_ORBIT;

        #endregion

        /// <summary>
        /// Returns the total amount of ticks elapsed.
        /// </summary>
        public long Ticks { get; private set; }

        public string FileFormat { get { return Ticks.ToString (); } }

        public GameClock () {
            Ticks = 1000;
        }

        public GameClock (long ticks) {
            Ticks = ticks;
        }

        public TickType Advance () {
            TickType types = TickType.Heartbeat;

            Ticks++;
            if (Ticks % TICKS_PER_PARTIAL == 0) {
                types |= TickType.Partial;
            }
            if (Ticks % TICKS_PER_ROTATION == 0) {
                types |= TickType.Rotation;
            }
            if (Ticks % TICKS_PER_CYCLE == 0) {
                types |= TickType.Cycle;
            }
            if (Ticks % TICKS_PER_ORBIT == 0) {
                types |= TickType.Orbit;
            }

            return types;
        }

        public void ResetCalendarTo (long ticks) {
            Ticks = ticks;
        }

        public override string ToString () {
            return string.Format ("[GameClock: Ticks={0}]", Ticks);
        }
    }
}
