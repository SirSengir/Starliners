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
using System.Linq;
using BLibrary.Serialization;
using BLibrary.Util;
using System.Runtime.Serialization;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class FleetRelocator : SerializableObject, IHoldable {

        public bool SuppressEntityClick {
            get {
                return true;
            }
        }

        [GameData (Remote = true)]
        public Fleet Fleet {
            get;
            private set;
        }

        public FleetRelocator (IWorldAccess access, Fleet fleet)
            : base (access, (ulong)access.Rand.Next ()) {
            Fleet = fleet;
        }

        #region Serialization

        public FleetRelocator (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public IHoldable OnMapClick (Player player, Vect2f coordinates, ControlState control) {
            INavPoint nav = player.Access.Entities.Values.Where (p => p.Bounding.IntersectsWith (coordinates)).OfType<INavPoint> ().FirstOrDefault ();
            if (nav == null) {
                return this;
            }

            Fleet.Relocate (nav);
            return null;
        }
    }
}

