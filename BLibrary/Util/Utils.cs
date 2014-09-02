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
using System.Text;

namespace BLibrary.Util {

    public static class Utils {
        public static int WeightedRandom (Random rand, int[] weight) {
            return weight [rand.Next (weight.Length)];
        }

        public static bool IsWithin (Vect2d location, Rect2f rectangle) {
            return IntersectsWith (location.X, location.Y, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        public static bool IsWithin (Vect2d location, double minX, double maxX, double minY, double maxY) {
            if ((location.X < minX || location.X > maxX)
                || (location.Y < minY || location.Y > maxY))
                return false;

            return true;
        }

        public static bool IntersectsWith (double pointX, double pointY, double rectX, double rectY, double rectW, double rectH) {

            if ((pointX < rectX || pointX > rectX + rectW)
                || (pointY < rectY || pointY > rectY + rectH))
                return false;

            return true;

        }

        public static bool IntersectsWith (Rect2f point, Rect2f rect) {
            return IntersectsWith (point.Coordinates.X, point.Coordinates.Y, point.Width, point.Height, rect.Coordinates.X, rect.Coordinates.Y, rect.Width, rect.Height);
        }

        public static bool IntersectsWith (double pointX, double pointY, double pointW, double pointH, double rectX, double rectY, double rectW, double rectH) {
            if (pointX + pointW < rectX
                || pointY + pointH < rectY)
                return false;
            if (pointX > rectX + rectW
                || pointY > rectY + rectH)
                return false;

            return true;

        }

        public static string BuildName (params string[] parts) {
            if (parts.Length <= 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder ();
            for (int i = 0; i < parts.Length; i++) {
                if (builder.Length > 0)
                    builder.Append (LibraryConstants.NAME_DELIM);
                builder.Append (parts [i]);
            }

            return builder.ToString ();
        }
    }
}
