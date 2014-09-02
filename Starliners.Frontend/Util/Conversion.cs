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
using System.Collections.Generic;
using OpenTK.Graphics;
using QuickFont;
using OpenTK.Input;
using BLibrary.Util;

namespace Starliners.Util {

    public class Conversion {
        static Conversion () {
            GenerateKeyMappings ();
        }

        public static float DegreeToRadians (double degree) {
            return (float)(degree * (Math.PI / 180));
        }

        public static float RadiansToDegree (double radian) {
            return (float)(radian / (Math.PI / 180));
        }

        public static Color4 ColourToColor4 (Colour colour) {
            return new Color4 (colour.R, colour.G, colour.B, colour.A);
        }

        public static Colour Color4ToColour (Color4 color) {
            return new Colour ((byte)(color.R * Byte.MaxValue),
                (byte)(color.G * Byte.MaxValue),
                (byte)(color.B * Byte.MaxValue),
                (byte)(color.A * Byte.MaxValue));
        }

        public static QFontAlignment AlignmentToQFontAlignment (Alignment align) {
            switch (align) {
                case Alignment.Center:
                    return QFontAlignment.Centre;
                case Alignment.Right:
                    return QFontAlignment.Right;
                default:
                case Alignment.Left:
                    return QFontAlignment.Left;
            }
        }

        static Dictionary<Key, KeysU> _keyMappings = new Dictionary<Key, KeysU> ();

        static void GenerateKeyMappings () {
            _keyMappings [Key.A] = KeysU.A;
            _keyMappings [Key.B] = KeysU.B;
            _keyMappings [Key.C] = KeysU.C;
            _keyMappings [Key.E] = KeysU.E;
            _keyMappings [Key.F] = KeysU.F;
            _keyMappings [Key.G] = KeysU.G;
            _keyMappings [Key.H] = KeysU.H;
            _keyMappings [Key.I] = KeysU.I;
            _keyMappings [Key.L] = KeysU.L;
            _keyMappings [Key.M] = KeysU.M;
            _keyMappings [Key.N] = KeysU.N;
            _keyMappings [Key.P] = KeysU.P;
            _keyMappings [Key.Q] = KeysU.Q;
            _keyMappings [Key.R] = KeysU.R;
            _keyMappings [Key.S] = KeysU.S;
            _keyMappings [Key.T] = KeysU.T;
            _keyMappings [Key.Y] = KeysU.Y;
            _keyMappings [Key.Escape] = KeysU.Escape;
        }

        public static KeysU KeyToKeysU (Key key) {
            return _keyMappings.ContainsKey (key) ? _keyMappings [key] : KeysU.Unknown;
        }
    }
}
