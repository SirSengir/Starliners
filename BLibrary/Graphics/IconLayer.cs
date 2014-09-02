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

using BLibrary.Util;


namespace BLibrary.Graphics {

    /// <summary>
    /// Represents an icon layer.
    /// </summary>
    public struct IconLayer {
        /// <summary>
        /// The index as registered with the SpriteManager.
        /// </summary>
        public uint Index;
        /// <summary>
        /// The colour to apply.
        /// </summary>
        public Colour Colour;
        /// <summary>
        /// The rotation to apply.
        /// </summary>
        public float Rotation;
        /// <summary>
        /// The translation to apply.
        /// </summary>
        public Vect2f Translation;

        public IconLayer (uint index)
            : this (index, Colour.White, 0, Vect2f.ZERO) {
        }

        public IconLayer (uint index, Colour colour)
            : this (index, colour, 0, Vect2f.ZERO) {
        }

        public IconLayer (uint index, Colour colour, float rotation, Vect2f translation) {
            Index = index;
            Colour = colour;
            Rotation = rotation;
            Translation = translation;
        }

        #region Helper functions

        /// <summary>
        /// Creates simple icons from the given array of icon indices.
        /// </summary>
        /// <returns>The icons.</returns>
        /// <param name="indeces">Indeces.</param>
        public static IconLayer[] CreateIcons (uint[] indeces) {
            IconLayer[] icons = new IconLayer[indeces.Length];
            for (int i = 0; i < indeces.Length; i++) {
                icons [i] = new IconLayer (indeces [i]);
            }
            return icons;
        }

        #endregion
    }
}

