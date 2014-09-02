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

    public sealed class MathUtils {

        public static double CalcAngle (Vect2d location, double destinationX, double destinationY) {
            return Math.Atan2 (destinationY - location.Y, destinationX - location.X);
        }

        public static float GetDistanceBetween (Vect2d location, Vect2d other) {
            return GetDistanceBetween (location.X, location.Y, other.X, other.Y);
        }

        public static float GetDistanceBetween (double locationX, double locationY, double otherX, double otherY) {
            double underRadical = Math.Pow ((otherX - locationX), 2) + Math.Pow ((otherY - locationY), 2);
            return (float)Math.Sqrt (underRadical);
        }

        static Vect2f GetLineIntersection (Vect2f start0, Vect2f end0, Vect2f start1, Vect2f end1) {
            // Get A,B,C of first line - points : ps1 to pe1
            float A1 = end0.Y - start0.Y;
            float B1 = start0.X - end0.X;
            float C1 = A1 * start0.X + B1 * start0.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = end1.Y - start1.Y;
            float B2 = start1.X - end1.X;
            float C2 = A2 * start1.X + B2 * start1.Y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                throw new System.Exception ("Lines are parallel");

            // now return the Vector2 intersection point
            return new Vect2f (
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );
        }

        public static double CalcAngle (Vect2d point0, Vect2d point1) {
            return Math.Atan2 (point1.Y - point0.Y, point1.X - point0.X);
        }

        /// <summary>
        /// Determines whether the given point is within the polygon described by the passed vector array.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsWithinPolygon (Vect2f point, Vect2f[] polygon) {
            bool isInside = false;

            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++) {

                if (((polygon [i].Y > point.Y) != (polygon [j].Y > point.Y)) &&
                    (point.X < (polygon [j].X - polygon [i].X) * (point.Y - polygon [i].Y) / (polygon [j].Y - polygon [i].Y) + polygon [i].X)) {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        /// <summary>
        /// Determines whether the given point is within the polygon described by the passed vector array.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsWithinPolygon (Vect2f point, Vect2i[] polygon) {
            bool isInside = false;

            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++) {

                if (((polygon [i].Y > point.Y) != (polygon [j].Y > point.Y)) &&
                    (point.X < (polygon [j].X - polygon [i].X) * (point.Y - polygon [i].Y) / (polygon [j].Y - polygon [i].Y) + polygon [i].X)) {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        /// <summary>
        /// Returns the outer bounds of the given polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static Rect2i GetPolygonBounds (Vect2i[] polygon) {
            int minX = 0, minY = 0;
            int maxX = 0, maxY = 0;
            for (int i = 0; i < polygon.Length; i++) {
                if (polygon [i].X < minX)
                    minX = polygon [i].X;
                if (polygon [i].Y < minY)
                    minY = polygon [i].Y;
                if (polygon [i].X > maxX)
                    maxX = polygon [i].X;
                if (polygon [i].Y > maxY)
                    maxY = polygon [i].Y;
            }

            return new Rect2i (minX, minY, maxX - minX, maxY - minY);
        }

        public static ulong ReadUlong (int[] store, int offset) {
            ulong extract = (ulong)(long)store [offset] << 48 | (ulong)(long)store [offset + 1] << 32 | (ulong)(long)store [offset + 2] << 16 | (ulong)(long)store [offset + 3];
            return extract;
        }

        public static void WriteUlong (ulong write, int[] store, int offset) {
            //ulong extract = (ulong)store [offset] << 48 | (ulong)store [offset + 1] << 32 | (ulong)store [offset + 2] << 16 | (ulong)store [offset + 3];
            store [offset] = (int)(write >> 48);
            store [offset + 1] = (int)(write >> 32);
            store [offset + 2] = (int)(write >> 16);
            store [offset + 3] = (int)write;
        }

        public static float AsAmplitude (float metronom) {
            return metronom < 0.5f ? 0.5f + metronom : 1.5f - metronom;
        }
    }
}
