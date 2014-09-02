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

﻿using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;

namespace BLibrary.Gui.Backgrounds {

    public sealed class BackgroundDynamic : Background {

        public Direction Edges {
            get { return _edges; }
            set {
                _edges = value;
                _borderSpaceY = _borderSpaceX = 0;
                if (_edges.HasFlag (Direction.North)) {
                    _borderSpaceY += _spriteSize.Y;
                }
                if (_edges.HasFlag (Direction.South)) {
                    _borderSpaceY += _spriteSize.Y;
                }
                if (_edges.HasFlag (Direction.West)) {
                    _borderSpaceX += _spriteSize.X;
                }
                if (_edges.HasFlag (Direction.East)) {
                    _borderSpaceX += _spriteSize.X;
                }

            }
        }

        Vect2i _spriteSize;
        Sprite _template;
        int _borderSpaceX;
        int _borderSpaceY;
        Direction _edges;
        Vect2i _size;
        Drawable _drawable;

        public BackgroundDynamic ()
            : this ("guiWindow") {
        }

        public BackgroundDynamic (string name)
            : this (GuiManager.Instance.GetGuiSprite (name)) {
        }

        BackgroundDynamic (Sprite template) {
            _template = template;
            _spriteSize = new Vect2i (
                (int)((float)template.SourceRect.Width / 3),
                (int)((float)template.SourceRect.Height / 3)
            );
            Edges = Direction.North | Direction.East | Direction.South | Direction.West;

        }

        public override void Render (Vect2i position, Vect2i size, RenderTarget target, RenderStates states, Colour colour) {
            base.Render (position, size, target, states, colour);

            // This needs to be cleaned up. Size should be a fixed property
            // of the used background copy.
            if (size != _size) {
                _drawable = CreateDrawable (size, colour);
                _size = size;
            }

            states.Transform.Translate (position);
            _drawable.Draw (target, states);
        }

        SpriteBatch CreateDrawable (Vect2i size, Colour colour) {

            Rect2i[] sources = new Rect2i[9];
            Rect2f[] destinations = new Rect2f[9];
            Vect2i partsize = new Vect2i (_template.SourceRect.Width / 3, _template.SourceRect.Height / 3);

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    sources [i * 3 + j] = new Rect2i (_template.SourceRect.Left + j * partsize.X, _template.SourceRect.Top + i * partsize.Y, partsize.X, partsize.Y);
                }
            }

            Vect2i shiftLeft = new Vect2i ((Edges.HasFlag (Direction.West) ? partsize.X : 0), (Edges.HasFlag (Direction.North) ? partsize.Y : 0));
            Vect2i shiftRight = new Vect2i ((Edges.HasFlag (Direction.East) ? partsize.X : 0), (Edges.HasFlag (Direction.South) ? partsize.Y : 0));

            if (Edges.HasFlag (Direction.North)) {
                destinations [0] = new Rect2i (new Vect2i (), partsize);
                destinations [1] = new Rect2i (new Vect2i (shiftLeft.X, 0), new Vect2i (size.X - shiftRight.X - shiftLeft.X, partsize.Y));
                destinations [2] = new Rect2i (new Vect2i (size.X - shiftRight.X, 0), partsize);

            }
            if (Edges.HasFlag (Direction.West)) {
                destinations [3] = new Rect2i (new Vect2i (0, shiftLeft.Y), new Vect2i (partsize.X, size.Y - shiftRight.Y - shiftLeft.Y));
            }

            destinations [4] = new Rect2i (shiftLeft, size - shiftLeft - shiftRight);

            if (Edges.HasFlag (Direction.East)) {
                destinations [5] = new Rect2i (new Vect2i (size.X - shiftRight.X, shiftLeft.Y), new Vect2i (partsize.X, size.Y - shiftRight.Y - shiftLeft.Y));
            }

            if (Edges.HasFlag (Direction.South)) {
                destinations [6] = new Rect2i (new Vect2i (0, size.Y - shiftRight.Y), partsize);
                destinations [7] = new Rect2i (new Vect2i (shiftLeft.X, size.Y - shiftRight.Y), new Vect2i (size.X - shiftRight.X - shiftLeft.X, partsize.Y));
                destinations [8] = new Rect2i (new Vect2i (size.X - shiftRight.X, size.Y - shiftRight.Y), partsize);
            }

            return new SpriteBatch (_template.Texture, sources, destinations, new Colour[] { colour });
        }

        public override Background Copy () {
            return new BackgroundDynamic (_template) {
                Colour = Colour,
                Edges = _edges,
                Shadow = Shadow != null ? Shadow.Copy () : null
            };
        }
    }
}
