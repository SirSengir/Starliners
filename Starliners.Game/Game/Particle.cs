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

﻿using BLibrary.Serialization;
using System.Runtime.Serialization;
using BLibrary.Util;
using System;

namespace Starliners.Game {

    [Serializable]
    public class Particle : SerializableObject {
        public double Age {
            get;
            set;
        }

        [GameData (Remote = true)]
        public int MaxAge {
            get;
            set;
        }

        [GameData (Remote = true)]
        public Vect2d Location {
            get;
            protected set;
        }

        [GameData (Remote = true)]
        public ParticleId Type {
            get;
            protected set;
        }

        [GameData (Remote = true)]
        public int Seed {
            get;
            set;
        }

        public virtual bool HasSound {
            get {
                return false;
            }
        }

        public virtual string Sound {
            get {
                return string.Empty;
            }
        }

        public Particle (IWorldAccess access, Vect2d location, ParticleId type)
            : base (access) {
            Location = location;
            Type = type;
            MaxAge = 46;
        }

        #region Serialization

        public Particle (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public virtual void Update (double elapsed) {
            Age += elapsed * 10;
        }
    }
}
