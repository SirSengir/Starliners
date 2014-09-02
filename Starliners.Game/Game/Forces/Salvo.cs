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
    public sealed class Salvo : ISerializable {

        public long Tick {
            get;
            private set;
        }

        public int OriginSlot {
            get;
            private set;
        }

        public Volley Shot {
            get;
            private set;
        }

        public Colour Colour {
            get;
            private set;
        }

        public int TargetSlot {
            get;
            private set;
        }

        public DamageReport Damage {
            get;
            private set;
        }

        public int Loot {
            get;
            private set;
        }

        public Salvo (long tick, int origin, Volley shot, Colour colour) {
            Tick = tick;
            OriginSlot = origin;
            Shot = shot;
            Colour = colour;
        }

        #region Serialization

        public Salvo (SerializationInfo info, StreamingContext context) {
            Tick = info.GetInt64 ("Tick");
            OriginSlot = info.GetInt32 ("Origin");
            Shot = (Volley)info.GetValue ("Shot", typeof(Volley));
            Colour = new Colour (info.GetInt32 ("Colour"));
            TargetSlot = info.GetInt32 ("Target");
            Damage = (DamageReport)info.GetValue ("Damage", typeof(DamageReport));
            Loot = info.GetInt32 ("Loot");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Tick", Tick);
            info.AddValue ("Origin", OriginSlot);
            info.AddValue ("Shot", Shot, typeof(Volley));
            info.AddValue ("Colour", Colour.ToInteger ());
            info.AddValue ("Target", TargetSlot);
            info.AddValue ("Damage", Damage, typeof(DamageReport));
            info.AddValue ("Loot", Loot);
        }

        #endregion

        public void RegisterHit (int target, DamageReport damage, int loot) {
            TargetSlot = target;
            Damage = damage;
            Loot = loot;
        }
    }
}

