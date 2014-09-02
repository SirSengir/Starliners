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

namespace BLibrary.Util {

    [Serializable ()]
    public struct Rect2i {
        Vect2i _coordinates;
        Vect2i _size;
        Vect2i _center;

        #region Properties

        public Vect2i Coordinates { get { return _coordinates; } }

        public Vect2i Size { get { return _size; } }

        public Vect2i Center { get { return _center; } }

        public int Left { get { return _coordinates.X; } }

        public int Right { get { return _coordinates.X + _size.X; } }

        public int Top { get { return _coordinates.Y; } }

        public int Bottom { get { return _coordinates.Y + _size.Y; } }

        public int Width { get { return _size.X; } }

        public int Height { get { return _size.Y; } }

        #endregion

        public Rect2i (int x, int y, int width, int height)
            : this (new Vect2i (x, y), new Vect2i (width, height)) {
        }

        public Rect2i (float x, float y, float width, float height)
            : this (new Vect2i (x, y), new Vect2i (width, height)) {
        }

        public Rect2i (Vect2i coordinates, int width, int height)
            : this (coordinates, new Vect2i (width, height)) {
        }

        public Rect2i (int x, int y, Vect2i size)
            : this (new Vect2i (x, y), size) {
        }

        public Rect2i (Vect2i coordinates, Vect2i size) {
            _coordinates = coordinates;
            _size = size;
            _center = new Vect2i (coordinates.X + (size.X / 2), coordinates.Y + (size.Y / 2));
        }

        public bool IntersectsWith (Vect2i point) {
            if (point.X < _coordinates.X || point.Y < _coordinates.Y)
                return false;
            if (point.X > _coordinates.X + _size.X
                || point.Y > _coordinates.Y + _size.Y)
                return false;

            return true;
        }

        public bool IntersectsWith (Vect2f point) {
            if (point.X < _coordinates.X || point.Y < _coordinates.Y)
                return false;
            if (point.X > _coordinates.X + _size.X
                || point.Y > _coordinates.Y + _size.Y)
                return false;

            return true;
        }

        public bool IntersectsWith (Rect2i rect) {
            if (rect.Coordinates.X + rect.Size.X < _coordinates.X
                || rect.Coordinates.Y + rect.Size.Y < _coordinates.Y)
                return false;
            if (rect.Coordinates.X > _coordinates.X + _size.X
                || rect.Coordinates.Y > _coordinates.Y + _size.Y)
                return false;

            return true;
        }

        public bool IntersectsWith (Rect2f rect) {
            if (rect.Coordinates.X + rect.Size.X < _coordinates.X
                || rect.Coordinates.Y + rect.Size.Y < _coordinates.Y)
                return false;
            if (rect.Coordinates.X > _coordinates.X + _size.X
                || rect.Coordinates.Y > _coordinates.Y + _size.Y)
                return false;

            return true;
        }

        public override string ToString () {
            return string.Format ("[Rect2i: Coordinates={0}, Size={1}]", Coordinates, Size);
        }

        public static implicit operator Rect2f (Rect2i cast) {
            return new Rect2f (cast.Coordinates, cast.Size);
        }

        public static implicit operator System.Drawing.Rectangle (Rect2i cast) {
            return new System.Drawing.Rectangle (cast.Left, cast.Top, cast.Width, cast.Height);
        }

        #region Math

        public static Rect2i operator + (Rect2i lhs, Rect2i rhs) {
            return new Rect2i (lhs.Coordinates + rhs.Coordinates, lhs.Size + rhs.Size);
        }

        #endregion
    }
}
