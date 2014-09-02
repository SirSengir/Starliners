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
using System.Linq;

namespace BLibrary.Audio {

    sealed class SoundCollection : IClipContainer {

        public bool Randomized {
            get;
            set;
        }

        readonly Random _rand;
        readonly SoundClip[] _clips;
        int _count = 0;

        public SoundCollection (IEnumerable<SoundClip> clips) {
            _rand = new Random ();
            _clips = clips.ToArray ();
            _count = 0;
        }

        /// <summary>
        /// Returns the current clip and advances the collections internal counter.
        /// </summary>
        public SoundClip Clip {
            get {
                if (Randomized) {
                    return _clips [_rand.Next (_clips.Length)];
                }

                SoundClip clip = _clips [_count];

                if (_count < _clips.Length - 1) {
                    _count++;
                } else {
                    _count = 0;
                }

                return clip;
            }
        }
    }
}

