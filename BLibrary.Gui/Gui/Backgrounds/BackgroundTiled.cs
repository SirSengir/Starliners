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
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;

namespace BLibrary.Gui.Backgrounds {

    public sealed class BackgroundTiled : Background {

        #region Cache

        static Dictionary<string, Dictionary<Vect2i, Texture>> _cachedTextures = new Dictionary<string, Dictionary<Vect2i, Texture>> ();

        #endregion

        #region Fields

        string _ident;
        Sprite _template;
        Vect2i _size;
        Drawable _drawable;

        #endregion

        public Direction Edges {
            get;
            set;
        }

        public BackgroundTiled ()
            : this ("guiWindow") {
        }

        public BackgroundTiled (string name)
            : this (name, GuiManager.Instance.GetGuiSprite (name)) {
        }

        BackgroundTiled (string ident, Sprite template) {
            _ident = ident;
            _template = template;
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

        Drawable CreateDrawable (Vect2i size, Colour colour) {

            // Use cached texture if available.
            if (_cachedTextures.ContainsKey (_ident) && _cachedTextures [_ident].ContainsKey (size)) {
                return new Sprite (_cachedTextures [_ident] [size], new Rect2i (0, 0, size), colour);
            }

            Rect2i[] sources = new Rect2i[9];
            Rect2i[] destinations = new Rect2i[9];
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

            // Create mappings of source to destination quads.
            List<Rect2i> batchSources = new List<Rect2i> ();
            List<Rect2f> batchDestinations = new List<Rect2f> ();
            for (int i = 0; i < sources.Length; i++) {
                Rect2i source = sources [i];
                Rect2i dest = destinations [i];
                for (int j = dest.Coordinates.X; j < dest.Coordinates.X + dest.Size.X; j += source.Size.X) {
                    for (int k = dest.Coordinates.Y; k < dest.Coordinates.Y + dest.Size.Y; k += source.Size.Y) {
                        batchSources.Add (source);
                        batchDestinations.Add (new Rect2i (new Vect2i (j, k), source.Size));
                    }
                }

            }

            return new SpriteBatch (_template.Texture, batchSources.ToArray (), batchDestinations.ToArray (), new Colour[] { colour });
        }

        public override Background Copy () {
            return new BackgroundTiled (_ident, _template) {
                Colour = Colour,
                Edges = Edges,
                Shadow = Shadow != null ? Shadow.Copy () : null
            };
        }
    }
}
