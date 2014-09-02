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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Starliners;

namespace BLibrary.Graphics {

    public sealed class VertexBuffer : IDisposable {
        #region Properties

        public Texture Texture {
            get;
            private set;
        }

        public int VAOId {
            get;
            private set;
        }

        public bool IsDrawable {
            get { return _vertexCount > 0; }
        }

        #endregion

        PrimitiveType _primitive;
        int _vertexCount = 0;
        BVertex[] _vertices = new BVertex[1000];
        int _vboId = -1;
        // Indicates whether this gl resources have been inited.
        // (VertexBuffer.Load() was called at least once.
        bool _glInited = false;

        #region Constructor

        public VertexBuffer (Texture texture)
            : this (PrimitiveType.Triangles, texture) {
        }

        public VertexBuffer (PrimitiveType primitive, Texture texture) {
            _primitive = primitive;
            Texture = texture;
            VAOId = GlWrangler.GenVertexArray ();
            GL.GenBuffers (1, out _vboId);
        }

        #endregion

        #region IDisposable

        ~VertexBuffer ()
        {
            Dispose (false);
        }

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
                ErrorCode error = GL.GetError ();
                if (error != ErrorCode.NoError) {
                    GameAccess.Interface.GameConsole.Rendering ("An error lingered before attempting to dispose a VertexBuffer: " + error.ToString ());
                }

                // We can only do this when disposing manually,
                // since the Finalizer runs on a different thread
                // and there will usually not be an OpenGL context available.
                if (_glInited) {
                    GL.DeleteBuffers (1, ref _vboId);
                    GlWrangler.DeleteVertexArray (VAOId);
                }
                _disposed = true;

                error = GL.GetError ();
                if (error != ErrorCode.NoError) {
                    GameAccess.Interface.GameConsole.Rendering ("Encountered an error while attempting to dispose a VertexBuffer: " + error.ToString ());
                }

            } else {
                GlManager.EnqueueOrphan (this);
            }

        }

        #endregion

        public void Reset () {
            _vertexCount = 0;
        }

        /// <summary>
        /// Adds a vertex to this buffer.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="normal">Normal.</param>
        /// <param name="textureCoord">Texture coordinate.</param>
        /// <param name="rgba">Rgba.</param>
        public void AddVertex (Vector3 point, Vector3 normal, Vector2 textureCoord, int rgba) {
            if (_vertexCount + 1 >= _vertices.Length) {
                var newArray = new BVertex[_vertices.Length * 2];
                Array.Copy (_vertices, newArray, _vertexCount);
                _vertices = newArray;
            }

            _vertices [_vertexCount] = new BVertex (point, normal, textureCoord, rgba);
            _vertexCount++;
        }

        /// <summary>
        /// Pushes the stored vertices to the GPU.
        /// </summary>
        public void Load () {
            if (!IsDrawable) {
                return;
            }

            GlWrangler.BindVertexArray (VAOId);
            GL.BindBuffer (BufferTarget.ArrayBuffer, _vboId);

            GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)(_vertexCount * BlittableValueType.StrideOf (_vertices)), _vertices, BufferUsageHint.StaticDraw);
            _glInited = true;

            /*
            int size;
            GL.GetBufferParameter (BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (_vertexCount * BlittableValueType.StrideOf (_vertices) != size) {
                throw new ApplicationException ("Vertex data not uploaded correctly");
            }*/

            GL.EnableClientState (ArrayCap.VertexArray);
            GL.EnableClientState (ArrayCap.TextureCoordArray);
            GL.EnableClientState (ArrayCap.ColorArray);

            GL.VertexPointer (3, VertexPointerType.Float, BlittableValueType.StrideOf (_vertices), new IntPtr (0));
            //GL.NormalPointer (NormalPointerType.Float, BlittableValueType.StrideOf (_vertices), new IntPtr (12));
            GL.TexCoordPointer (2, TexCoordPointerType.Float, BlittableValueType.StrideOf (_vertices), new IntPtr (24));
            GL.ColorPointer (4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf (_vertices), new IntPtr (32));

            GlWrangler.BindVertexArray (0);
        }

        public void Draw () {
            GL.DrawArrays (_primitive, 0, _vertexCount);
        }
    }
}