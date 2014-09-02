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

namespace Starliners.Game {
    /// <summary>
    /// Represents current movement as a struct of current speed and current angle of the moving object.
    /// </summary>
    [Serializable]
    public struct Movement : ISerializable {
        public static readonly Movement NEUTRAL = new Movement (Vect2f.ZERO, Math.PI / 2);

        #region Properties

        public Vect2d Speed {
            get;
            set;
        }

        public double Angle {
            get;
            set;
        }

        #endregion

        public Movement (Vect2d speed, double angle)
            : this () {
            Speed = speed;
            Angle = angle;
        }

        #region Serialization

        public Movement (SerializationInfo info, StreamingContext context)
            : this () {
            Speed = new Vect2d (info.GetDouble ("SpeedX"), info.GetDouble ("SpeedY"));
            Angle = info.GetDouble ("Angle");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("SpeedX", Speed.X);
            info.AddValue ("SpeedY", Speed.Y);
            info.AddValue ("Angle", Angle);
        }

        #endregion

        public override int GetHashCode () {
            unchecked {
                int hash = 17;
                hash = hash * 23 + Speed.GetHashCode ();
                hash = hash * 23 + Angle.GetHashCode ();
                return hash;
            }
        }

        public override bool Equals (object obj) {
            if (!(obj is Movement)) {
                return false;
            }

            return this.Equals ((Movement)obj);
        }

        public bool Equals (Movement other) {
            return Speed == other.Speed && Angle == other.Angle;
        }

        public static bool operator == (Movement lhs, Movement rhs) {
            return lhs.Speed == rhs.Speed && lhs.Angle == rhs.Angle;
        }

        public static bool operator != (Movement lhs, Movement rhs) {
            return !(lhs == rhs);
        }

        public override string ToString () {
            return string.Format ("[Movement: Speed={0}, Angle={1}]", Speed, Angle);
        }
    }
}

