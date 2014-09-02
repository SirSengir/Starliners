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
    public enum Alignment {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Center = 1 << 2,
        Top = 1 << 3,
        Bottom = 1 << 4,
        Justify = 1 << 5
    }

    public sealed class Alignments {
        /// <summary>
        /// Aligns according to horizontal alignment.
        /// </summary>
        /// <param name="elementWidth"></param>
        /// <param name="contentWidth"></param>
        /// <param name="hAlign"></param>
        /// <returns>Offset to translate by as int. (Integer prevents fuzzy rendering.)</returns>
        public static int GetAlignH (float elementWidth, float contentWidth, Alignment hAlign) {
            if (hAlign == Alignment.Center)
                return (int)(elementWidth - contentWidth) / 2;
            else if (hAlign == Alignment.Right)
                return (int)(elementWidth - contentWidth);
            else
                return 0;

        }

        /// <summary>
        /// Aligns according to vertical alignment.
        /// </summary>
        /// <param name="elementHeight"></param>
        /// <param name="contentHeight"></param>
        /// <param name="vAlign"></param>
        /// <returns>Offset to translate by as int. (Integer prevents fuzzy rendering.)</returns>
        public static int GetAlignV (float elementHeight, float contentHeight, Alignment vAlign) {
            if (vAlign == Alignment.Center)
                return (int)(elementHeight - contentHeight) / 2;
            else if (vAlign == Alignment.Bottom)
                return (int)(elementHeight - contentHeight);
            else
                return 0;

        }
    }
}
