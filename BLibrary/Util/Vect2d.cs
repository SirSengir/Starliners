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

namespace BLibrary.Util {

    [Serializable]
    public struct Vect2d : ISerializable {
        public static int ByteSize { get { return 2 * sizeof(double); } }

        double _x;
        double _y;

        #region Properties

        public double X {
            get { return _x; }
        }

        public double Y {
            get { return _y; }
        }

        #endregion

        public Vect2d (double xCoord, double yCoord) {
            _x = xCoord;
            _y = yCoord;
        }

        public Vect2d (SerializationInfo info, StreamingContext context) {
            _x = info.GetDouble ("X");
            _y = info.GetDouble ("Y");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("X", _x);
            info.AddValue ("Y", _y);
        }

        public override string ToString () {
            return string.Format ("[Vect2f: X={0}, Y={1}]", X, Y);
        }

        public override int GetHashCode () {
            unchecked {
                int hash = 17;
                hash = hash * 23 + _x.GetHashCode ();
                hash = hash * 23 + _y.GetHashCode ();
                return hash;
            }

        }

        #region Casting

        public static explicit operator Vect2f (Vect2d cast) {
            return new Vect2f ((float)cast.X, (float)cast.Y);
        }

        public static explicit operator Vect2i (Vect2d cast) {
            return new Vect2i ((int)cast.X, (int)cast.Y);
        }

        public static Vect2d operator + (Vect2d lhs, Vect2d rhs) {
            return new Vect2d (lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vect2d operator - (Vect2d lhs, Vect2d rhs) {
            return new Vect2d (lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vect2d operator * (Vect2d lhs, Vect2d rhs) {
            return new Vect2d (lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static Vect2d operator * (Vect2i lhs, Vect2d rhs) {
            return new Vect2d (lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static Vect2d operator * (Vect2d lhs, float rhs) {
            return new Vect2d (lhs.X * rhs, lhs.Y * rhs);
        }

        #endregion

        #region Comparison

        public override bool Equals (object obj) {
            if (!(obj is Vect2d))
                return false;

            return this.Equals ((Vect2d)obj);
        }

        public bool Equals (Vect2d other) {
            return X == other.X && Y == other.Y;
        }

        public static bool operator == (Vect2d lhs, Vect2d rhs) {
            return lhs.Equals (rhs);
        }

        public static bool operator != (Vect2d lhs, Vect2d rhs) {
            return !(lhs == rhs);
        }

        #endregion

    }
}

