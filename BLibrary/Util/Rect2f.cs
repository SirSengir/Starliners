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

﻿using BLibrary.Util;

namespace BLibrary.Util {

    public struct Rect2f {
        Vect2f _coordinates;
        Vect2f _size;
        Vect2f _center;

        #region Properties

        public Vect2f Coordinates { get { return _coordinates; } }

        public Vect2f Size { get { return _size; } }

        public Vect2f Center { get { return _center; } }

        public float Left { get { return _coordinates.X; } }

        public float Right { get { return _coordinates.X + _size.X; } }

        public float Top { get { return _coordinates.Y; } }

        public float Bottom { get { return _coordinates.Y + _size.Y; } }

        public float Width { get { return _size.X; } }

        public float Height { get { return _size.Y; } }

        #endregion

        public Rect2f (Vect2i coordinates, Vect2f size)
            : this (new Vect2f (coordinates.X, coordinates.Y), size) {
        }

        public Rect2f (float x, float y, Vect2i size)
            : this (new Vect2f (x, y), new Vect2f (size.X, size.Y)) {
        }

        public Rect2f (float x, float y, float width, float height)
            : this (new Vect2f (x, y), new Vect2f (width, height)) {
        }

        public Rect2f (int x, int y, int width, int height)
            : this (new Vect2f (x, y), new Vect2f (width, height)) {
        }

        public Rect2f (Vect2f coordinates, float width, float height)
            : this (coordinates, new Vect2f (width, height)) {
        }

        public Rect2f (Vect2f coordinates, Vect2f size) {
            _coordinates = coordinates;
            _size = size;
            _center = new Vect2f (coordinates.X + (size.X / 2), coordinates.Y + (size.Y / 2));
        }

        public override string ToString () {
            return string.Format ("[Rect2f: Coordinates={0}, Size={1}, Center={2}]", Coordinates, Size, Center);
        }

        public bool IntersectsWith (Vect2d point) {
            return Utils.IntersectsWith (point.X, point.Y, _coordinates.X, _coordinates.Y, _size.X, _size.Y);
        }

        public bool IntersectsWith (double x, double y) {
            return Utils.IntersectsWith (x, y, _coordinates.X, _coordinates.Y, _size.X, _size.Y);
        }

        public bool IntersectsWith (Rect2f rect) {
            return Utils.IntersectsWith (rect.Coordinates.X, rect.Coordinates.Y, rect.Size.X, rect.Size.Y, _coordinates.X, _coordinates.Y, _size.X, _size.Y);
        }
    }
}
