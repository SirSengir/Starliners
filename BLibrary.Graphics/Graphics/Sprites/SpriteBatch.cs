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
using BLibrary.Util;

namespace BLibrary.Graphics.Sprites {

    public sealed class SpriteBatch : Drawable {
        #region Properties

        public Rect2f SourceRect0 {
            get { return _quads [0].SourceRect; }
        }

        #endregion

        Quad[] _quads;

        #region Constructors

        public SpriteBatch (Texture texture, Rect2i[] sources, Rect2f[] destinations, Colour[] colours)
            : base (texture, 1) {
            if (destinations.Length != sources.Length) {
                throw new SystemException ("A SpriteBatch requires all given arrays to be of the same length.");
            }

            _quads = Quad.CreateQuads (sources, destinations, colours);
        }

        public SpriteBatch (Texture texture, Rect2i[] sources, Rect2f[] destinations)
            : base (texture, 1) {
            if (destinations.Length != sources.Length) {
                throw new SystemException ("A SpriteBatch requires all given arrays to be of the same length.");
            }

            _quads = Quad.CreateQuads (sources, destinations, Colour.White);
        }

        #endregion

        protected override Quad[] GetQuads (int buffer) {
            return _quads;
        }

        protected override Rect2f CalculateBounds () {

            float minX = 0, minY = 0, maxX = 0, maxY = 0;
            for (int i = 0; i < _quads.Length; i++) {
                Rect2f destination = _quads [i].DestinationRect;
                if (destination.Left < minX) {
                    minX = destination.Left;
                }
                if (destination.Top < minY) {
                    minY = destination.Top;
                }
                if (destination.Left + destination.Width > maxX) {
                    maxX = destination.Left + destination.Width;
                }
                if (destination.Top + destination.Height > maxY) {
                    maxY = destination.Top + destination.Height;
                }
            }
            return new Rect2f (minX, minY, maxX, maxY);
        }
    }
}

