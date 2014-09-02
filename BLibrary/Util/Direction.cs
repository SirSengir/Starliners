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

    [Flags]
    public enum Direction {
        Unknown = 0,
        NorthWest = 1 << 0,
        North = 1 << 1,
        NorthEast = 1 << 2,
        East = 1 << 3,
        SouthEast = 1 << 4,
        South = 1 << 5,
        SouthWest = 1 << 6,
        West = 1 << 7
    }

    public static class Directions {

        public static readonly Direction[] VALUES = (Direction[])Enum.GetValues (typeof(Direction));

        public static readonly Direction[] CARDINAL = new Direction[] {
            Direction.West,
            Direction.North,
            Direction.East,
            Direction.South
        };
        public static readonly Direction[] CORNERS = new Direction[] {
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.SouthEast,
            Direction.SouthWest
        };

        public static readonly int[] OFFSETS_X = new int[] { 0, -1, 0, 1, 1, 1, 0, -1, -1 };
        public static readonly int[] OFFSETS_Y = new int[] { 0, -1, -1, -1, 0, 1, 1, 1, 0 };

        public static bool IsCornerFor (this Direction direction, Direction other) {
            if (direction == Direction.NorthWest && (other.HasFlag (Direction.North) || other.HasFlag (Direction.West)))
                return true;
            if (direction == Direction.NorthEast && (other.HasFlag (Direction.North) || other.HasFlag (Direction.East)))
                return true;
            if (direction == Direction.SouthWest && (other.HasFlag (Direction.South) || other.HasFlag (Direction.West)))
                return true;
            if (direction == Direction.SouthEast && (other.HasFlag (Direction.South) || other.HasFlag (Direction.East)))
                return true;

            return false;
        }

        public static int GetXOffset (this Direction direction) {
            switch (direction) {
                case Direction.NorthWest:
                    return -1;
                case Direction.North:
                    return 0;
                case Direction.NorthEast:
                case Direction.East:
                case Direction.SouthEast:
                    return 1;
                case Direction.South:
                    return 0;
                case Direction.SouthWest:
                case Direction.West:
                    return -1;
            }
            return 0;
        }

        public static int GetYOffset (this Direction direction) {
            switch (direction) {
                case Direction.NorthWest:
                case Direction.North:
                case Direction.NorthEast:
                    return -1;
                case Direction.East:
                    return 0;
                case Direction.SouthEast:
                case Direction.South:
                case Direction.SouthWest:
                    return 1;
                case Direction.West:
                    return 0;
            }
            return 0;
        }

    }
}
