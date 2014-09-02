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

namespace BLibrary.Graphics.Sprites {

    public sealed class Sprite : Drawable {
        #region Properties

        /// <summary>
        /// Gets or sets the texture of this Drawable.
        /// </summary>
        /// <remarks>Causes a buffer update.</remarks>
        /// <value>The texture.</value>
        public Texture Texture {
            get { return _texture; }
            private set {
                _texture = value;
                DirtyBuffers = true;
            }
        }

        public Colour Colour {
            get { return _quads [0].Colour; }
            set {
                if (_quads [0].Colour == value) {
                    return;
                }
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

        #endregion

        Quad[] _quads;
        Texture _texture;

        #region Constructor

        public Sprite (Texture texture)
            : this (texture, new Rect2i (0, 0, texture.Size), Colour.White) {
        }

        public Sprite (Texture texture, Rect2i sourceRect)
            : this (texture, sourceRect, Colour.White) {
        }

        public Sprite (Texture texture, Rect2i sourceRect, Colour colour)
            : base (texture, 1) {

            _texture = texture;
            _quads = new Quad[] { Quad.CreateQuad (sourceRect, colour) };
        }

        public Sprite (Sprite copy)
            : this (copy.Texture, copy) {
        }

        public Sprite (Texture texture, Sprite copy)
            : base (texture, copy) {

            _texture = texture;
            _quads = new Quad[copy._quads.Length];
            for (int i = 0; i < _quads.Length; i++) {
                _quads [i] = copy._quads [i].Copy ();
            }

            Rotation = copy.Rotation;
            Origin = copy.Origin;
            Position = copy.Position;
            Scale = copy.Scale;
        }

        #endregion

        protected override Rect2f CalculateBounds () {
            return _quads [0].DestinationRect;
        }

        protected override Quad[] GetQuads (int buffer) {
            return _quads;
        }
    }
}
