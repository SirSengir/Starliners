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
using System.Runtime.Serialization;
using BLibrary.Util;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class Regen : ISerializable {

        public long Tick {
            get;
            private set;
        }

        public int OriginSlot {
            get;
            private set;
        }

        public int Healed {
            get;
            private set;
        }

        public StructureLayer Layer {
            get;
            private set;
        }

        public int TargetSlot {
            get;
            private set;
        }

        public Regen (long tick, int origin, int healed, StructureLayer layer) {
            Tick = tick;
            OriginSlot = origin;
            Healed = healed;
            Layer = layer;
        }

        #region Serialization

        public Regen (SerializationInfo info, StreamingContext context) {
            Tick = info.GetInt64 ("Tick");
            OriginSlot = info.GetInt32 ("Origin");
            Healed = info.GetInt32 ("Healed");
            Layer = (StructureLayer)info.GetInt32 ("Layer");
            TargetSlot = info.GetInt32 ("Target");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Tick", Tick);
            info.AddValue ("Origin", OriginSlot);
            info.AddValue ("Healed", Healed);
            info.AddValue ("Layer", (int)Layer);
            info.AddValue ("Target", TargetSlot);
        }

        #endregion

        public void RegisterEffected (int target, int max) {
            TargetSlot = target;
            Healed = Healed > max ? max : Healed;
        }
    }
}

