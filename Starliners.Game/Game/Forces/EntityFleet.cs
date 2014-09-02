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
using BLibrary.Util;
using BLibrary.Serialization;
using System.Runtime.Serialization;
using Starliners.Game.Forces;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class EntityFleet : Entity {

        #region Constants

        static readonly Vect2f BOUNDING_SIZE = new Vect2f (1f, 0.5f);
        static readonly Vect2f BOUNDING_OFFSET = new Vect2f (-0.5f, 0);

        #endregion

        #region Properties

        public override Blueprint Blueprint {
            get {
                return _blueprint;
            }
        }

        public override Vect2f BoundingSize {
            get {
                return BOUNDING_SIZE;
            }
        }

        protected override Vect2f BoundingOffset {
            get {
                return BOUNDING_OFFSET;
            }
        }

        public override Faction Owner {
            get {
                return Contained.Owner;
            }
        }

        [GameData (Remote = true, Key = "Contained")]
        public Fleet Contained {
            get;
            private set;
        }

        #endregion

        [GameData (Remote = true, Key = "Blueprint")]
        Blueprint _blueprint;

        public EntityFleet (IWorldAccess access, Blueprint blueprint, Fleet fleet)
            : base (access, string.Format ("entity_fleet_{0}", fleet.Serial), blueprint) {
            _blueprint = blueprint;
            Contained = fleet;
        }

        #region Serialization

        public EntityFleet (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion
    }
}

