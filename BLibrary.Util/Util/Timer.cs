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

namespace BLibrary.Util {

    public sealed class Timer {
        int _waitTicks = 0;
        long _startTick = 0;

        public long Elapsed {
            get {
                return GetTicks () - _startTick;
            }
        }

        public bool IsDelayed {
            get {
                return Elapsed < _waitTicks;
            }
        }

        public long Remaining {
            get {
                return _waitTicks - Elapsed;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSimulator.Game.Timer"/> class.
        /// </summary>
        /// <param name='waitTicks'>
        /// Wait ticks (Will be multiplied by Math.Pow(10, 4).
        /// </param>
        public Timer (int waitTicks) {
            _waitTicks = waitTicks * (int)Math.Pow (10, 4);
        }

        public void Reset () {
            _startTick = GetTicks ();
        }

        public long GetTicks () {
            return DateTime.Now.Ticks;
        }

        public static int GetTrueTicks (int ticks) {
            return ticks * (int)Math.Pow (10, 4);
        }
    }
}
