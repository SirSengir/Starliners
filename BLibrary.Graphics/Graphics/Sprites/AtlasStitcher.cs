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
using System.Drawing;
using BLibrary.Util;

namespace BLibrary.Graphics.Sprites {

    sealed class AtlasStitcher : IDisposable {
        #region Constants

        static readonly Vect2i STITCHED_SHEET_SIZE = new Vect2i (4096, 4096);

        #endregion

        #region Classes

        sealed class StitchNode {
            readonly StitchNode[] _children = new StitchNode[2];
            bool _leaf = false;
            bool _occupied = false;
            // Indicates whether this node is occupied.
            public Rect2i Area { get; set; }

            public StitchNode (Rect2i area) {
                Area = area;
            }

            public StitchNode Insert (Rect2i source) {
                // If this node is already filled, check children.
                if (_leaf) {
                    StitchNode node = _children [0].Insert (source);
                    return node ?? _children [1].Insert (source);
                }

                if (_occupied)
                    return null;

                // Source material is too large for this node, so we skip it.
                if (source.Width > Area.Width || source.Height > Area.Height)
                    return null;

                // Source material fits perfectly, so we are the one.
                if (source.Width == Area.Width && source.Height == Area.Height) {
                    _occupied = true;
                    return this;
                }

                // We don't match exactly, so we need to split child nodes.
                int deltaWidth = Area.Width - source.Width;
                int deltaHeight = Area.Height - source.Height;
                if (deltaWidth > deltaHeight) {
                    _children [0] = new StitchNode (new Rect2i (Area.Left, Area.Top, 
                        source.Width, Area.Height));
                    _children [1] = new StitchNode (new Rect2i (Area.Left + source.Width, Area.Top, 
                        Area.Width - source.Width, Area.Height));
                } else {
                    _children [0] = new StitchNode (new Rect2i (Area.Left, Area.Top, 
                        Area.Width, source.Height));
                    _children [1] = new StitchNode (new Rect2i (Area.Left, Area.Top + source.Height, 
                        Area.Width, Area.Height - source.Height));
                }

                _leaf = true;
                return _children [0].Insert (source);
            }
        }

        /// <summary>
        /// Simple class for deferring adjustment on icon mappings.
        /// </summary>
        sealed class MappingAdjustment {
            readonly IconMapping _mapping;
            readonly Rect2i[] _adjustment;

            public MappingAdjustment (IconMapping mapping, Rect2i[] adjustment) {
                _mapping = mapping;
                _adjustment = adjustment;
            }

            public void Execute (Texture stitched) {
                _mapping.Adjust (stitched, _adjustment);
            }
        }

        #endregion

        readonly Texture _stitched;
        readonly Bitmap _bitmap;
        readonly System.Drawing.Graphics _graphics;
        readonly StitchNode _entry;
        readonly List<MappingAdjustment> _adjustments = new List<MappingAdjustment> ();

        public AtlasStitcher () {
            _bitmap = new Bitmap (STITCHED_SHEET_SIZE.X, STITCHED_SHEET_SIZE.Y);//Texture.GetMaximumSize (), Texture.GetMaximumSize ());
            _graphics = System.Drawing.Graphics.FromImage (_bitmap);
            _graphics.Clear (Color.Transparent);

            // Decreased slightly to avoid some weirdness at the edges. The problem runs deeper though I think.
            _entry = new StitchNode (new Rect2i (0, 0, _bitmap.Width - 1, _bitmap.Height - 1));

            RenderTexture target = new RenderTexture (_bitmap.Width, _bitmap.Height);
            target.Clear (Colour.Transparent);
            target.Display ();
            _stitched = target.Texture;
            target.Dispose ();
        }

        /// <summary>
        /// Stitches a mapping onto the atlas texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="bitmap"></param>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public IconMapping StitchMapping (Texture texture, Bitmap bitmap, IconMapping mapping) {

            Rect2i[] modified = new Rect2i[mapping.Parts.Length];

            for (int i = 0; i < mapping.Parts.Length; i++) {
                modified [i] = _entry.Insert (mapping.Parts [i]).Area;
                _graphics.DrawImage (bitmap, modified [i], mapping.Parts [i], GraphicsUnit.Pixel);
            }

            // We defer adjustments until the new atlas texture is actually complete.
            _adjustments.Add (new MappingAdjustment (mapping, modified));
            return mapping;
        }

        public void Flush () {
            _graphics.Flush ();
        }

        /// <summary>
        /// Updates the texture and adjusts existing icon mappings.
        /// </summary>
        public void UpdateTexture () {
            _bitmap.Save ("finalized_texture.bmp");
            _stitched.Update (_bitmap);

            // Adjust existing mappings to refer to the newly created texture.
            foreach (MappingAdjustment adjustment in _adjustments) {
                adjustment.Execute (_stitched);
            }
        }

        public void Dispose () {
            _bitmap.Dispose ();
            _graphics.Dispose ();
        }
    }
}

