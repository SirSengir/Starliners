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
    public struct Vect2f : ISerializable {

        public static readonly Vect2f ZERO = new Vect2f (0, 0);

        public static int ByteSize { get { return 2 * sizeof(float); } }

        float x;
        float y;

        #region Properties

        public float X { get { return x; } }

        public float Y { get { return y; } }

        #endregion

        public Vect2f (float xCoord, float yCoord) {
            this.x = xCoord;
            this.y = yCoord;
        }

        public Vect2f (SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException ("info");

            x = info.GetSingle ("X");
            y = info.GetSingle ("Y");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("X", x);
            info.AddValue ("Y", y);
        }

        public override string ToString () {
            return string.Format ("[Vect2f: X={0}, Y={1}]", X, Y);
        }

        public override int GetHashCode () {
            unchecked {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode ();
                hash = hash * 23 + y.GetHashCode ();
                return hash;
            }
        }

        #region Casting

        public static explicit operator Vect2i (Vect2f cast) {
            return new Vect2i ((int)cast.X, (int)cast.Y);
        }

        #endregion

        #region Math

        public static implicit operator Vect2d (Vect2f cast) {
            return new Vect2d (cast.X, cast.Y);
        }

        public static Vect2f operator + (Vect2f lhs, Vect2f rhs) {
            return new Vect2f (lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vect2f operator - (Vect2f lhs, Vect2f rhs) {
            return new Vect2f (lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vect2f operator - (Vect2f lhs, Vect2i rhs) {
            return new Vect2f (lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vect2f operator + (Vect2i lhs, Vect2f rhs) {
            return new Vect2f (lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vect2f operator * (Vect2f lhs, Vect2f rhs) {
            return new Vect2f (lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static Vect2f operator * (Vect2i lhs, Vect2f rhs) {
            return new Vect2f (lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static Vect2f operator * (Vect2f lhs, float rhs) {
            return new Vect2f (lhs.X * rhs, lhs.Y * rhs);
        }

        public static Vect2f operator / (Vect2f lhs, float rhs) {
            return new Vect2f (lhs.X / rhs, lhs.Y / rhs);
        }

        public static Vect2f operator / (float lhs, Vect2f rhs) {
            return new Vect2f (lhs / rhs.X, lhs / rhs.Y);
        }

        public static Vect2f operator / (Vect2f lhs, Vect2f rhs) {
            return new Vect2f (lhs.X / rhs.X, lhs.Y / rhs.Y);
        }

        public static bool operator > (Vect2f lhs, int rhs) {
            return lhs.X + lhs.Y > rhs;
        }

        public static bool operator < (Vect2f lhs, int rhs) {
            return lhs.X + lhs.Y < rhs;
        }

        public static Vect2f operator % (Vect2f lhs, Vect2f rhs) {
            return new Vect2f (lhs.X % rhs.X, lhs.Y % rhs.Y);
        }

        #endregion

        #region Comparison

        public override bool Equals (object obj) {
            if (!(obj is Vect2f))
                return false;

            return this.Equals ((Vect2f)obj);
        }

        public bool Equals (Vect2f other) {
            return X == other.X && Y == other.Y;
        }

        public static bool operator == (Vect2f lhs, Vect2f rhs) {
            return lhs.Equals (rhs);
        }

        public static bool operator != (Vect2f lhs, Vect2f rhs) {
            return !(lhs == rhs);
        }

        #endregion

    }

}
