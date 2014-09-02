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

﻿using BLibrary.Graphics.Sprites;
using BLibrary.Util;


namespace BLibrary.Graphics {

    sealed class Rectangle : Drawable {
        #region Properties

        public Colour Colour {
            get { return _quads [0].Colour; }
            set {
                if (_quads [0].Colour == value)
                    return;
                _quads [0].Colour = value;
                DirtyBuffers = true;
            }
        }

        public Rect2i SourceRect {
            get { return _quads [0].SourceRect; }
            set {
                _quads [0].SourceRect = value;
                // Rebuild the destination rectangle as well.
                _quads [0].DestinationRect = new Rect2i (0, 0, _quads [0].SourceRect.Width, _quads [0].SourceRect.Height);
                DirtyBuffers = true;
            }
        }

        public Vect2f Size {
            get { return _quads [0].DestinationRect.Size; }
            set {
                if (_quads [0].DestinationRect.Size != value) {
                    _quads [0].DestinationRect = new Rect2f (0, 0, value.X, value.Y);
                    DirtyBuffers = true;
                }
            }
        }

        #endregion

        Quad[] _quads;

        public Rectangle (Vect2i size)
            : base (SpriteManager.Instance.DummySprite.Texture, 1) {

            _quads = new Quad[] { Quad.CreateQuad (SpriteManager.Instance.DummySprite.SourceRect, new Rect2i (0, 0, size), Colour.White) };
        }

        protected override Quad[] GetQuads (int buffer) {
            return _quads;
        }

        protected override Rect2f CalculateBounds () {
            return _quads [0].DestinationRect;
        }
    }
}
