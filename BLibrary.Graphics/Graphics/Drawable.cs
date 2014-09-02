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

namespace BLibrary.Graphics {

    public abstract class Drawable : Transformable, IDrawable, IDisposable {
        float _alpha = 1.0f;

        /// <summary>
        /// Gets or sets the alpha value to use when drawing this sprite.
        /// </summary>
        /// <remarks>Causes a buffer update. Multiplied with the texture's alpha.</remarks>
        /// <value>The alpha.</value>
        public float Alpha {
            get { return _alpha; }
            set {
                if (_alpha != value) {
                    _alpha = value;
                    DirtyBuffers = true;
                }
            }
        }

        Rect2f _bounds;

        /// <summary>
        /// Gets the local bounds of this Drawable.
        /// </summary>
        /// <value>The local bounds.</value>
        public override sealed Rect2f LocalBounds {
            get {
                VerifyClean ();
                return _bounds;
            }
        }

        protected bool DirtyBuffers {
            get;
            set;
        }

        VertexBuffer[] _buffers;

        #region Constructor

        public Drawable (Texture texture, int buffers) {
            CreateBuffers (texture, buffers);
        }

        public Drawable (Texture[] textures, int buffers) {
            if (buffers != textures.Length) {
                throw new ArgumentException ("Texture count and buffer count need to match.");
            }
            CreateBuffers (textures, buffers);
        }

        public Drawable (Texture texture, Drawable copy) {
            CreateBuffers (texture, copy._buffers.Length);
        }

        #endregion

        #region IDisposable

        bool _disposed = false;

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                for (int i = 0; i < _buffers.Length; i++) {
                    _buffers [i].Dispose ();
                }
            }
            _disposed = true;
        }

        #endregion

        public void Draw (RenderTarget target, RenderStates states) {
            // Update the buffer if it is dirty.
            VerifyClean ();
            states.Transform *= Transform;
            target.Draw (_buffers, states);
        }

        void VerifyClean () {
            if (DirtyBuffers) {

                RenderTarget.VBVertUpdates++;
                _bounds = CalculateBounds ();
                ResetVBOs ();
                PopulateVBOs ();
                LoadVBOs ();

                DirtyBuffers = false;
            }
        }

        void CreateBuffers (Texture texture, int amount) {
            if (texture == null) {
                throw new ArgumentNullException ("Require a texture to create buffers.");
            }

            _buffers = new VertexBuffer[amount];
            for (int i = 0; i < _buffers.Length; i++) {
                _buffers [i] = new VertexBuffer (texture);
            }
            DirtyBuffers = true;
        }

        void CreateBuffers (Texture[] textures, int amount) {
            _buffers = new VertexBuffer[amount];
            for (int i = 0; i < _buffers.Length; i++) {
                if (textures [i] == null) {
                    throw new ArgumentNullException ("Require a texture to create buffers.");
                }

                _buffers [i] = new VertexBuffer (textures [i]);
            }
            DirtyBuffers = true;
        }

        protected abstract Rect2f CalculateBounds ();

        protected abstract Quad[] GetQuads (int buffer);

        void ResetVBOs () {
            for (int i = 0; i < _buffers.Length; i++) {
                _buffers [i].Reset ();
            }
        }

        void LoadVBOs () {
            for (int i = 0; i < _buffers.Length; i++) {
                _buffers [i].Load ();
            }
        }

        void DrawVBOs () {
            for (int i = 0; i < _buffers.Length; i++) {
                _buffers [i].Draw ();
            }
        }

        void PopulateVBOs () {
            for (int i = 0; i < _buffers.Length; i++) {
                Quad[] quads = GetQuads (i);
                for (int j = 0; j < quads.Length; j++) {
                    quads [j].Alpha = _alpha;
                    quads [j].Upload (_buffers [i]);
                }
            }
        }
    }
}

