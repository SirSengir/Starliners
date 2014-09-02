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

﻿using csvorbis;

namespace BLibrary.Audio {

    sealed class SoundTask {
        public string Ident {
            get;
            private set;
        }

        public VorbisFile Clip {
            get { return _clip.Vorbis; }
        }

        public bool IsCompleted {
            get;
            set;
        }

        public bool IsLooped {
            get;
            set;
        }

        public bool MustFade {
            get;
            set;
        }

        public bool PropertiesChanged {
            get;
            set;
        }

        public float Volume {
            get { return _volume; }
            set {
                _volume = value;
                PropertiesChanged = true;
            }
        }

        float _volume;
        SoundClip _clip;

        public SoundTask (string ident, SoundClip clip) {
            Ident = ident;
            _clip = clip;
            Volume = clip.BaseGain;
        }
    }
}

