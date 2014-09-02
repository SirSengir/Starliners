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

﻿using System.Drawing;
using BLibrary.Util;

namespace BLibrary.Graphics.Text {

    /// <summary>
    /// Contains basic information on text style and colour.
    /// </summary>
    public struct TextFormat {
        public FontStyle Style;
        public Colour Colour;

        public TextFormat (Colour colour)
            : this (FontStyle.Regular, colour) {
        }

        public TextFormat (FontStyle style, Colour colour) {
            Style = style;
            Colour = colour;
        }

        public override string ToString () {
            return string.Format ("[TextFormat: Style={0}, Colour={1}]", Style, Colour);
        }

        public override int GetHashCode () {
            unchecked {
                int hash = 17;
                hash = hash * 23 + Style.GetHashCode ();
                hash = hash * 23 + Colour.GetHashCode ();
                return hash;
            }
        }

        public override bool Equals (object obj) {
            if (!(obj is TextFormat))
                return false;

            return this.Equals ((TextFormat)obj);
        }

        public bool Equals (TextFormat other) {
            return Style == other.Style && Colour == other.Colour;
        }

        public static bool operator == (TextFormat lhs, TextFormat rhs) {
            return lhs.Style == rhs.Style && lhs.Colour == rhs.Colour;
        }

        public static bool operator != (TextFormat lhs, TextFormat rhs) {
            return !(lhs == rhs);
        }
    }
}

