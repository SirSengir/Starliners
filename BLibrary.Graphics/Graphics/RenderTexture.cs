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
using BLibrary.Graphics.Sprites;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Graphics {

    public sealed class RenderTexture : RenderTarget, IDisposable {
        #region Properties

        public override Vect2i Size {
            get {
                return Texture.ActualSize;
            }
        }

        /// <summary>
        /// The texture this RenderTarget draws to.
        /// </summary>
        /// <value>The texture.</value>
        public Texture Texture {
            get;
            private set;
        }

        #endregion

        int _colourId = -1;
        int _depthId = -1;

        #region Constructor

        public RenderTexture (int edge)
            : this (new Vect2i (edge, edge)) {
        }

        public RenderTexture (int width, int height)
            : this (new Vect2i (width, height)) {
        }

        public RenderTexture (Vect2i size) {
            CreateBuffers (size, false);
            DefaultView = new View ((Vect2i)(Texture.ActualSize / 2), Texture.ActualSize);
            View = DefaultView;
        }

        #endregion

        #region IDisposable

        ~RenderTexture () {
            Dispose (false);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="BInterface.Graphics.RenderTexture"/> object. DOES NOT RELEASE THE CREATED TEXTURE!
        /// </summary>
        /// <remarks>
        /// THIS WILL NOT DISPOSE THE CONTAINED TEXTURE! DISPOSE OF THE TEXTURE SEPERATELY IF NEEDED (not wrapped in another Drawable)
        /// Call <see cref="Dispose"/> when you are finished using the <see cref="BInterface.Graphics.RenderTexture"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="BInterface.Graphics.RenderTexture"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="BInterface.Graphics.RenderTexture"/> so the garbage collector can reclaim the memory that the
        /// <see cref="BInterface.Graphics.RenderTexture"/> was occupying.
        /// </remarks>
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
                GL.DeleteFramebuffer (FBOId);
                _disposed = true;

                ErrorCode error = GL.GetError ();
                if (error != ErrorCode.NoError) {
                    GameAccess.Interface.GameConsole.Rendering ("Encountered an error while attempting to dispose a VertexBuffer: " + error.ToString ());
                }

            } else {
                GlManager.EnqueueOrphan (this);
            }
        }

        #endregion

        void CreateBuffers (Vect2i size, bool depthBuffer) {

            GL.Ext.BindFramebuffer (FramebufferTarget.FramebufferExt, 0);

            PushGLStates ();

            // Create Color Tex
            _colourId = GL.GenTexture ();
            GL.BindTexture (TextureTarget.Texture2D, _colourId);
            GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Create Depth Tex
            if (depthBuffer) {
                _depthId = GL.GenTexture ();
                GL.BindTexture (TextureTarget.Texture2D, _depthId);
                GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            }

            // Create a FBO and attach the textures
            FBOId = GL.Ext.GenFramebuffer ();
            GL.Ext.BindFramebuffer (FramebufferTarget.FramebufferExt, FBOId);
            GL.Ext.FramebufferTexture2D (FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, _colourId, 0);
            if (depthBuffer) {
                GL.Ext.FramebufferTexture2D (FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthId, 0);
            }

            #region Check Error State
            switch (GL.Ext.CheckFramebufferStatus (FramebufferTarget.FramebufferExt)) {
                case FramebufferErrorCode.FramebufferCompleteExt:
				//Console.WriteLine ("FBO: The framebuffer is complete and valid for rendering.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
				//Console.WriteLine ("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
				//Console.WriteLine ("FBO: There are no attachments.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
				//Console.WriteLine ("FBO: Attachments are of different size. All attachments must have the same width and height.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
				//Console.WriteLine ("FBO: The color attachments have different format. All color attachments must have the same format.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
				//Console.WriteLine ("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
				//Console.WriteLine ("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                    break;
                case FramebufferErrorCode.FramebufferUnsupportedExt:
				//Console.WriteLine ("FBO: This particular FBO configuration is not supported by the implementation.");
                    break;
                default:
				//Console.WriteLine ("FBO: Status unknown. (yes, this is really bad.)");
                    break;
            }

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger (GetPName.MaxColorAttachmentsExt, out queryinfo [0]);
            GL.GetInteger (GetPName.AuxBuffers, out queryinfo [1]);
            GL.GetInteger (GetPName.MaxDrawBuffers, out queryinfo [2]);
            GL.GetInteger (GetPName.Stereo, out queryinfo [3]);
            GL.GetInteger (GetPName.Samples, out queryinfo [4]);
            GL.GetInteger (GetPName.Doublebuffer, out queryinfo [5]);
            //Console.WriteLine ("max. ColorBuffers: " + queryinfo [0] + " max. AuxBuffers: " + queryinfo [1] + " max. DrawBuffers: " + queryinfo [2] +
            //"\nStereo: " + queryinfo [3] + " Samples: " + queryinfo [4] + " DoubleBuffer: " + queryinfo [5]);

            //Console.WriteLine ("Last GL Error: " + GL.GetError ());

            #endregion Test for Error

            GL.Ext.BindFramebuffer (FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO
            GL.BindTexture (TextureTarget.Texture2D, 0); // bind default texture
            Texture = new Texture (_colourId, size.X, size.Y);

            PopGLStates ();
        }

        /// <summary>
        /// Prepares the texture for rendering.
        /// </summary>
        public void Display () {
            Texture.IsFlipped = true;
        }

        public static RenderTexture CreateRenderTextureFrom (Texture texture, Rect2i source) {
            RenderTexture render = new RenderTexture (source.Size);
            Sprite drawable = new Sprite (texture, source);
            render.Draw (drawable);
            render.Display ();
            return render;
        }
    }
}
