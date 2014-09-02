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

﻿
namespace BLibrary.Util {

    public sealed class SoundDefinition {

        public string Ident {
            get;
            private set;
        }

        public string Pattern {
            get;
            private set;
        }

        public bool IsCollection {
            get;
            set;
        }

        public bool IsRandomized {
            get;
            set;
        }

        public float BaseGain {
            get;
            private set;
        }

        public SoundDefinition (string ident, string pattern)
            : this (ident, pattern, 1.0f) {
        }

        public SoundDefinition (string ident, string pattern, float gain) {
            Ident = ident;
            Pattern = pattern;
            BaseGain = gain;
        }
    }
}

