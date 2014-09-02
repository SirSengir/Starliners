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

﻿using OpenTK;
using BLibrary.Util;

namespace BLibrary.Graphics {

    public sealed class Quad {
        #region Properties

        public Rect2i SourceRect {
            get { return _sourceRect; }
            set {
                _sourceRect = value;
            }
        }

        public Rect2f DestinationRect {
            get { return _destinationRect; }
            set { 
                _destinationRect = value;
            }
        }

        public Colour Colour {
            get { return _colour; }
            set {
                if (_colour == value) {
                    return;
                }
                _colour = value;
            }
        }

        public float Alpha {
            get {
                return _alpha;
            }
            set {
                _alpha = value;
            }
        }

        #endregion

        #region Fields

        Rect2i _sourceRect;
        Rect2f _destinationRect;

        Colour _colour;
        float _alpha = 1.0f;

        #endregion

        #region Constructor

        public Quad (Rect2i sourceRect, Rect2f destinationRect, Colour colour) {
            _sourceRect = sourceRect;
            _destinationRect = destinationRect;
            _colour = colour;
        }

        #endregion

        // I shouldn't be doing that.
        const float HERPDERP_FLOAT = 0.03f;

        /// <summary>
        /// Uploads the quad into the given vertex buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public void Upload (VertexBuffer buffer) {
            float tLeft = _sourceRect.Left + HERPDERP_FLOAT;// / Texture.ActualSize.X;
            float tTop = _sourceRect.Top + HERPDERP_FLOAT;// / Texture.ActualSize.Y;
            float tRight = _sourceRect.Left + _sourceRect.Width - 2 * HERPDERP_FLOAT;// / Texture.ActualSize.X;
            float tBottom = _sourceRect.Top + _sourceRect.Height - 2 * HERPDERP_FLOAT;// / Texture.ActualSize.Y;

            Vector2 tv1 = new Vector2 (tLeft, tTop);
            Vector2 tv2 = new Vector2 (tLeft, tBottom);
            Vector2 tv3 = new Vector2 (tRight, tBottom);
            Vector2 tv4 = new Vector2 (tRight, tTop);

            float vLeft = _destinationRect.Left - HERPDERP_FLOAT;
            float vTop = _destinationRect.Top - HERPDERP_FLOAT;
            float vRight = _destinationRect.Left + _destinationRect.Width + 2 * HERPDERP_FLOAT;
            float vBottom = _destinationRect.Top + _destinationRect.Height + 2 * HERPDERP_FLOAT;

            Vector3 v1 = new Vector3 (vLeft, vTop, 0);
            Vector3 v2 = new Vector3 (vLeft, vBottom, 0);
            Vector3 v3 = new Vector3 (vRight, vBottom, 0);
            Vector3 v4 = new Vector3 (vRight, vTop, 0);

            Vector3 normal = new Vector3 (0, 0, -1);
            int argb = _colour.ToBgra32WithAlpha (_alpha);

            buffer.AddVertex (v1, normal, tv1, argb);
            buffer.AddVertex (v2, normal, tv2, argb);
            buffer.AddVertex (v3, normal, tv3, argb);

            buffer.AddVertex (v1, normal, tv1, argb);
            buffer.AddVertex (v3, normal, tv3, argb);
            buffer.AddVertex (v4, normal, tv4, argb);
        }

        public Quad Copy () {
            return new Quad (SourceRect, DestinationRect, Colour) { Alpha = _alpha };
        }

        #region Helper functions

        public static Quad CreateQuad (Rect2i source, Colour colour) {
            return new Quad (source, new Rect2i (0, 0, source.Width, source.Height), colour);
        }

        public static Quad CreateQuad (Rect2i source, Rect2f destination, Colour colour) {
            return new Quad (source, destination, colour);
        }

        public static Quad[] CreateQuads (Rect2i[] sources, Colour colour) {
            Quad[] quads = new Quad[sources.Length];
            for (int i = 0; i < quads.Length; i++) {
                quads [i] = new Quad (sources [i], new Rect2f (0, 0, sources [i].Width, sources [i].Height), colour);
            }
            return quads;
        }

        public static Quad[] CreateQuads (Rect2i[] sources, Rect2f[] destinations, Colour colour) {
            Quad[] quads = new Quad[sources.Length];
            for (int i = 0; i < quads.Length; i++) {
                quads [i] = new Quad (sources [i], destinations [i], colour);
            }
            return quads;
        }

        public static Quad[] CreateQuads (Rect2i[] sources, Rect2f[] destinations, Colour[] colours) {
            Quad[] quads = new Quad[sources.Length];
            for (int i = 0; i < quads.Length; i++) {
                quads [i] = new Quad (sources [i], destinations [i], i < colours.Length ? colours [i] : colours [0]);
            }
            return quads;
        }

        #endregion
    }
}

