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

﻿using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using BLibrary.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace BLibrary.Graphics {

    public sealed class Texture : IDisposable {
        public static readonly bool SUPPORTS_NPOT = false;

        #region Properties

        /// <summary>
        /// Gets the OpenGL texture id.
        /// </summary>
        /// <value>The identifier.</value>
        public int GlTexId {
            get;
            private set;
        }

        public Vect2i Size {
            get;
            private set;
        }

        public bool IsSmooth {
            get { return _isSmooth; }
            set {
                _isSmooth = value;

                // Set texture options accordingly in GL.
                GL.BindTexture (TextureTarget.Texture2D, GlTexId);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, _isSmooth ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, _isSmooth ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            }
        }

        public bool IsRepeated {
            get { return _isRepeated; }
            set {
                _isRepeated = value;

                GL.BindTexture (TextureTarget.Texture2D, GlTexId);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, _isRepeated ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, _isRepeated ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            }
        }

        public Vect2i ActualSize {
            get { return _actualSize; }
        }

        public bool IsFlipped {
            get;
            set;
        }

        #endregion

        bool _isRepeated = false;
        bool _isSmooth = false;
        Vect2i _actualSize;

        #region Constructors

        public Texture (int textureHandle, int width, int height) {
            Size = new Vect2i (width, height);
            _actualSize = new Vect2i (GetValidSize (width), GetValidSize (height));
            GlTexId = textureHandle;
        }

        public Texture (int width, int height) {
            Create (width, height);
        }

        public Texture (Bitmap image) {
            LoadFromBitmap (image);
        }

        public Texture (Stream stream) {
            LoadFromStream (stream);
        }

        #endregion

        #region IDisposable

        ~Texture () {
            Dispose (false);
        }

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        bool _disposed = false;

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                // We can only do this when disposing manually,
                // since the Finalizer runs on a different thread
                // and there will usually not be an OpenGL context available.
                int _texId = GlTexId;
                GL.DeleteTextures (1, ref _texId);
                _disposed = true;

                ErrorCode error = GL.GetError ();
                if (error != ErrorCode.NoError) {
                    throw new SystemException ("Encountered an error while attempting to dispose a VertexBuffer: " + error.ToString ());
                }

            } else {
                GlManager.EnqueueOrphan (this);
            }
        }

        #endregion

        #region Loading

        bool Create (int width, int height) {
            // Check if texture parameters are valid before creating it
            if ((width == 0) || (height == 0)) {
                Console.Out.WriteLine ("Failed to create texture, invalid size (" + width + "x" + height + ")");
                return false;
            }

            Vect2i actualSize = new Vect2i (GetValidSize (width), GetValidSize (height));
            // Check the maximum texture size
            int maxSize = GetMaximumSize ();
            if ((actualSize.X > maxSize) || (actualSize.Y > maxSize)) {
                Console.Out.WriteLine ("Failed to create texture, its internal size is too high "
                + "(" + actualSize.X + "x" + actualSize.X + ", "
                + "maximum is " + maxSize + "x" + maxSize + ")");
                return false;
            }

            // All the validity checks passed, we can store the new texture settings
            Size = new Vect2i (width, height);
            _actualSize = actualSize;
            IsFlipped = false;

            if (GlTexId == 0) {
                GlTexId = GL.GenTexture ();
            }

            GL.BindTexture (TextureTarget.Texture2D, GlTexId);
            GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, _actualSize.X, _actualSize.Y, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, _isRepeated ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, _isRepeated ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, _isSmooth ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, _isSmooth ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);

            return true;
        }

        bool LoadFromBitmap (Bitmap image) {
            if (!Create (image.Width, image.Height))
                return false;
            Update (image);
            //GL.Flush ();
            return true;
        }

        bool LoadFromStream (Stream stream) {
            if (stream == null)
                throw new ArgumentNullException ("stream");
            try {
                Bitmap bitmap = new Bitmap (stream);
                return LoadFromBitmap (bitmap);
            } catch (Exception ex) {
                Console.Out.WriteLine (ex.StackTrace);
            }

            return false;
        }

        public void Update (Bitmap image) {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle (0, 0, image.Width, image.Height);
            BitmapData data = image.LockBits (rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Update (data.Scan0, rect.Left, rect.Top, rect.Width, rect.Height);
            image.UnlockBits (data);
            image.Dispose ();
        }

        void Update (IntPtr pixels, int x, int y, int width, int height) {
            try {
                GL.BindTexture (TextureTarget.Texture2D, GlTexId);
                GL.TexSubImage2D (TextureTarget.Texture2D, 0, x, y, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            } catch (Exception ex) {
                Console.Out.WriteLine (ex.StackTrace);
            }
            IsFlipped = false;
        }

        public byte[] GetData () {
            byte[] pixels = new byte[ActualSize.X * ActualSize.Y * 4];
            GL.BindTexture (TextureTarget.Texture2D, GlTexId);
            GL.GetTexImage (TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            return pixels;
        }

        public System.Drawing.Bitmap ToBitmap () {
            byte[] pixels = GetData ();
            Bitmap image = new Bitmap (ActualSize.X, ActualSize.Y);
            for (int i = 0; i < ActualSize.Y; i++) {
                for (int j = 0; j < ActualSize.X; j++) {
                    int offset = (i * ActualSize.X + j) * 4;
                    image.SetPixel (j, i, System.Drawing.Color.FromArgb (pixels [offset + 3], pixels [offset + 2], pixels [offset + 1], pixels [offset + 0]));
                }
            }

            return image;
        }

        #endregion

        #region ToString

        public override string ToString () {
            return String.Format ("{0} (Id: {1})", GetType (), GlTexId);
        }

        #endregion

        #region Helper functions

        public static int GetValidSize (int size) {
            if (SUPPORTS_NPOT) {
                return size;
            }

            int pot = 1;
            while (pot < size) {
                pot *= 2;
            }

            return pot;
        }

        public static int GetValidSize (Vect2i size) {
            return size.X > size.Y ? GetValidSize (size.X) : GetValidSize (size.Y);
        }

        public static int GetMaximumSize () {
            return GL.GetInteger (GetPName.MaxTextureSize);
        }

        static float[] _texMatrix = new float[] {
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        };

        /// <summary>
        /// Binds the given texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="pixels"></param>
        public static bool Bind (Texture texture, bool pixels) {

            if (texture == null) {
                // Bind texture
                GL.BindTexture (TextureTarget.Texture2D, 0);
                // Reset the texture matrix
                GL.MatrixMode (MatrixMode.Texture);
                GL.LoadIdentity ();
                GL.MatrixMode (MatrixMode.Modelview);
                return false;
            }

            GL.BindTexture (TextureTarget.Texture2D, texture.GlTexId);

            if (texture.IsFlipped || pixels) {

                _texMatrix [0] = 1f;
                _texMatrix [5] = 0f;
                _texMatrix [13] = 0f;

                if (pixels) {
                    _texMatrix [0] = 1f / texture.ActualSize.X;
                    _texMatrix [5] = 1f / texture.ActualSize.Y;
                }
                if (texture.IsFlipped) {
                    _texMatrix [5] = -_texMatrix [5];
                    _texMatrix [13] = (float)texture.Size.Y / texture.ActualSize.Y;
                }

                GL.MatrixMode (MatrixMode.Texture);
                GL.LoadMatrix (_texMatrix);
                GL.MatrixMode (MatrixMode.Modelview);
                return true;
            }

            return false;
        }

        public static void ResetTextureMatrix () {
            GL.MatrixMode (MatrixMode.Texture);
            GL.LoadIdentity ();
            GL.MatrixMode (MatrixMode.Modelview);
        }

        #endregion
    }
}
