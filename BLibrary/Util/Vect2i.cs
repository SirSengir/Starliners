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

    [Serializable ()]
    public struct Vect2i : ISerializable {
        public static readonly Vect2i ZERO = new Vect2i (0, 0);
        int _x;
        int _y;

        #region Properties

        public int X { get { return _x; } }

        public int Y { get { return _y; } }

        #endregion

        public Vect2i (int xCoord, int yCoord) {
            _x = xCoord;
            _y = yCoord;
        }

        public Vect2i (double xCoord, double yCoord) {
            _x = (int)xCoord;
            _y = (int)yCoord;
        }

        public Vect2i (SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException ("info");

            _x = info.GetInt32 ("X");
            _y = info.GetInt32 ("Y");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("X", _x);
            info.AddValue ("Y", _y);
        }

        public override int GetHashCode () {
            unchecked {
                int hash = 17;
                hash = hash * 23 + _x.GetHashCode ();
                hash = hash * 23 + _y.GetHashCode ();
                return hash;
            }
        }

        public static Vect2i Parse (string text) {
            string[] tokens = text.Split ('x');
            if (tokens.Length != 2)
                throw new ArgumentException ("Cannot parse " + text + " to a Vect2i. Needs to be in the format XxY.");
            return new Vect2i (
                Int32.Parse (tokens [0]),
                Int32.Parse (tokens [1])
            );
        }

        public static implicit operator Vect2f (Vect2i cast) {
            return new Vect2f (cast.X, cast.Y);
        }

        public static implicit operator Vect2d (Vect2i cast) {
            return new Vect2d (cast.X, cast.Y);
        }

        public static Vect2i operator + (Vect2i lhs, Vect2i rhs) {
            return new Vect2i (lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vect2i operator - (Vect2i lhs, Vect2i rhs) {
            return new Vect2i (lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vect2i operator - (Vect2i rhs) {
            return new Vect2i (-rhs.X, -rhs.Y);
        }

        public static Vect2i operator * (Vect2i lhs, Vect2i rhs) {
            return new Vect2i (lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static Vect2d operator * (Vect2i lhs, double rhs) {
            return new Vect2d ((double)lhs.X * rhs, (double)lhs.Y * rhs);
        }

        public static Vect2f operator * (Vect2i lhs, float rhs) {
            return new Vect2f ((float)lhs.X * rhs, (float)lhs.Y * rhs);
        }

        public static Vect2i operator * (Vect2i lhs, int rhs) {
            return new Vect2i (lhs.X * rhs, lhs.Y * rhs);
        }

        public static Vect2f operator * (Vect2f factor, Vect2i oper) {
            return new Vect2f (factor.X * oper.X, factor.Y * oper.Y);
        }

        public static Vect2f operator / (Vect2i lhs, Vect2i rhs) {
            return new Vect2f ((float)lhs.X / rhs.X, (float)lhs.Y / rhs.Y);
        }

        public static Vect2f operator / (Vect2i lhs, float rhs) {
            return new Vect2f ((float)lhs.X / rhs, (float)lhs.Y / rhs);
        }

        public override bool Equals (object obj) {
            if (!(obj is Vect2i))
                return false;

            return this.Equals ((Vect2i)obj);
        }

        public bool Equals (Vect2i other) {
            return X == other.X && Y == other.Y;
        }

        public static bool operator == (Vect2i lhs, Vect2i rhs) {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }

        public static bool operator != (Vect2i lhs, Vect2i rhs) {
            return !(lhs == rhs);
        }

        public override string ToString () {
            return string.Format ("[Vect2i: X={0}, Y={1}]", X, Y);
        }
    }
}
