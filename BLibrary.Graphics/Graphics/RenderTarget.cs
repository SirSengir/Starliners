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
using BLibrary.Util;


namespace BLibrary.Graphics {

    public abstract class RenderTarget {

        #region Properties

        public static int DrawCalls { get; set; }

        public static int TextureChanges { get; set; }

        public static int VBVertUpdates { get; set; }

        public static int PushedGLStates { get; set; }

        public static int ViewChanges { get; set; }

        public static int FBOChanges { get; set; }

        public abstract Vect2i Size {
            get;
        }

        public View View {
            get { return _view; }
            set {
                _view = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// Returns the default view for this RenderTarget.
        /// </summary>
        public View DefaultView {
            get;
            protected set;
        }

        #endregion

        #region Fields

        View _view = new View ();
        bool _viewChanged = true;

        protected int FBOId = -1;
        static RenderTarget _lastActiveTarget = null;
        // The last active render target. When changed, some states may need to be reapplied.
        static bool _glStatesSet = false;
        static int _currentTextureId = -1;
        static BlendMode _lastBlendMode = BlendMode.Alpha;
        static int _lastFBO = -1;
        static int _lastVAO = -1;

        #endregion

        public Rect2i GetViewport (View view) {
            float width = Size.X;
            float height = Size.Y;
            Rect2f viewport = view.Port;

            return new Rect2i ((0.5f + width * viewport.Coordinates.X),
                (0.5f + height * viewport.Coordinates.Y),
                (width * viewport.Size.X),
                (height * viewport.Size.Y));

        }

        /// <summary>
        /// Maps the given screen pixels to world coordinates using the RenderTarget's current view.
        /// </summary>
        /// <returns>World coordinates corresponding to the given screen pixel.</returns>
        /// <param name="point">The screen pixels.</param>
        /// <param name="view">The view to use for the mapping.</param>
        public Vect2f MapPixelToCoords (Vect2i point) {
            return MapPixelToCoords (point, View);
        }

        /// <summary>
        /// Maps the given screen pixels to world coordinates.
        /// </summary>
        /// <returns>World coordinates corresponding to the given screen pixel.</returns>
        /// <param name="point">The screen pixels.</param>
        /// <param name="view">The view to use for the mapping.</param>
        public Vect2f MapPixelToCoords (Vect2i point, View view) {
            // First, convert from viewport coordinates to homogeneous coordinates
            Rect2i viewport = GetViewport (view);
            Vect2f normalized = new Vect2f (
                                    -1f + 2f * (point.X - viewport.Coordinates.X) / viewport.Size.X,
                                    1f - 2f * (point.Y - viewport.Coordinates.Y) / viewport.Size.Y);

            // Then transform by the inverse of the view matrix
            return (Vect2f)view.Inverse.TransformPoint (normalized);

        }

        /// <summary>
        /// Maps the given world coordinates to a pixel on screen using the RenderTarget's current view.
        /// </summary>
        /// <returns>Screen pixel corresponding to the given world coordinate.</returns>
        /// <param name="point">The world coordinates.</param>
        public Vect2i MapCoordsToPixel (Vect2f point) {
            return MapCoordsToPixel (point, View);
        }

        /// <summary>
        /// Maps the given world coordinates to a pixel on screen.
        /// </summary>
        /// <returns>Screen pixel corresponding to the given world coordinate.</returns>
        /// <param name="point">The world coordinates.</param>
        /// <param name="view">The view to use for the mapping.</param>
        public Vect2i MapCoordsToPixel (Vect2d point, View view) {
            // First, transform the point by the view matrix
            Vect2d normalized = view.Transform.TransformPoint (point);

            // Then convert to viewport coordinates
            Rect2i viewport = GetViewport (view);
            return new Vect2i (
                (normalized.X + 1f) / 2f * viewport.Size.X + viewport.Coordinates.X,
                (-normalized.Y + 1f) / 2f * viewport.Size.Y + viewport.Coordinates.Y);

        }

        /// <summary>
        /// Clears the RenderTarget to Black.
        /// </summary>
        public void Clear () {
            Clear (Colour.Black);
        }

        /// <summary>
        /// Clears the RenderTarget with the given colour.
        /// </summary>
        /// <param name="color">Color.</param>
        public void Clear (Colour color) {
            OnBeginDraw ();
            GL.ClearColor ((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
            GL.Clear (ClearBufferMask.ColorBufferBit);
            OnEndDraw ();
        }

        public void EnableScissor (Rect2i area) {
            GL.Scissor (area.Coordinates.X, Size.Y - area.Height - area.Coordinates.Y, area.Width, area.Height);
            GL.Enable (EnableCap.ScissorTest);
        }

        public void DisableScissor () {
            GL.Disable (EnableCap.ScissorTest);
        }

        #region Activating

        protected virtual void OnBeginDraw () {
            // Check whether we were the previously active render target.
            if (_lastActiveTarget != this) {
                _viewChanged = true;
                _lastActiveTarget = this;
            }
            // Switch to correct frame buffer.
            if (_lastFBO != FBOId) {
                FBOChanges++;
                GL.Ext.BindFramebuffer (FramebufferTarget.FramebufferExt, FBOId);
                _lastFBO = FBOId;
            }
        }

        protected virtual void OnEndDraw () {
        }

        #endregion

        #region Drawing

        public void Draw (IDrawable drawable) {
            drawable.Draw (this, RenderStates.DEFAULT);
        }

        public void Draw (IDrawable drawable, RenderStates states) {
            drawable.Draw (this, states);
        }

        public void Draw (VertexBuffer[] buffers, RenderStates states) {

            DrawCalls++;
            OnBeginDraw ();

            // Init gl states if not done yet.
            if (!_glStatesSet) {
                ResetGLStates ();
            }

            // Apply the transformation
            ApplyTransform (states.Transform);

            // Apply view if it changed
            if (_viewChanged) {
                ApplyCurrentView ();
            }

            if (_lastBlendMode != states.BlendMode) {
                ApplyBlendMode (states.BlendMode);
            }

            bool shaderApplied = false;
            for (int i = 0; i < buffers.Length; i++) {

                // Skip if the buffer has nothing to draw.
                if (!buffers [i].IsDrawable) {
                    continue;
                }

                if (_lastVAO != buffers [i].VAOId) {
                    GlWrangler.BindVertexArray (buffers [i].VAOId);
                    _lastVAO = buffers [i].VAOId;
                }

                // Apply texture
                if (buffers [i].Texture.GlTexId != _currentTextureId) {
                    ApplyTexture (buffers [i].Texture, true);
                    shaderApplied = false; // Rebind the shader if the texture has changed.
                }

                // Apply shader
                if (states.Shader != null && !shaderApplied) {
                    ApplyShader (states.Shader, states.Transform);
                    shaderApplied = true;
                }

                buffers [i].Draw ();

            }

            // Unbind shader
            if (states.Shader != null) {
                ApplyShader (null, states.Transform);
            }

            OnEndDraw ();
        }

        #endregion

        #region GL States

        public void ResetGLStates () {

            GL.Disable (EnableCap.CullFace);
            GL.Disable (EnableCap.Lighting);
            GL.Disable (EnableCap.DepthTest);
            GL.Disable (EnableCap.AlphaTest);
            GL.Enable (EnableCap.Texture2D);
            GL.Enable (EnableCap.Blend);
            GL.MatrixMode (MatrixMode.Modelview);
            GL.EnableClientState (ArrayCap.VertexArray);
            GL.EnableClientState (ArrayCap.ColorArray);
            GL.EnableClientState (ArrayCap.TextureCoordArray);
            GL.PolygonMode (MaterialFace.FrontAndBack, PolygonMode.Fill);
            _glStatesSet = true;

            ApplyBlendMode (BlendMode.Alpha);
            ApplyTransform (Transform.Identity);
            ApplyTexture (null, false);

            // Set the default view
            _viewChanged = true;

        }

        public void UnbindTexture () {
            ApplyTexture (null, false);
        }

        public void PushGLStates () {
            PushedGLStates++;
            GL.PushClientAttrib (ClientAttribMask.ClientAllAttribBits);
            GL.PushAttrib (AttribMask.AllAttribBits);
            GL.MatrixMode (MatrixMode.Modelview);
            GL.PushMatrix ();
            GL.MatrixMode (MatrixMode.Projection);
            GL.PushMatrix ();
            GL.MatrixMode (MatrixMode.Texture);
            GL.PushMatrix ();
            ResetGLStates ();
        }

        public void PopGLStates () {
            GL.MatrixMode (MatrixMode.Projection);
            GL.PopMatrix ();
            GL.MatrixMode (MatrixMode.Modelview);
            GL.PopMatrix ();
            GL.MatrixMode (MatrixMode.Texture);
            GL.PopMatrix ();
            GL.PopClientAttrib ();
            GL.PopAttrib ();
        }

        #endregion

        #region Apply States

        void ApplyCurrentView () {
            ViewChanges++;

            Rect2i port = GetViewport (_view);
            int top = Size.Y - (port.Coordinates.Y + port.Size.Y);
            GL.Viewport (port.Coordinates.X, top, port.Size.X, port.Size.Y);

            GL.MatrixMode (MatrixMode.Projection);
            GL.LoadMatrix (_view.Transform.Matrix);
            GL.MatrixMode (MatrixMode.Modelview);

            _viewChanged = false;
        }

        void ApplyBlendMode (BlendMode mode) {
            _lastBlendMode = mode;

            switch (mode) {
                case BlendMode.Add:
                    GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                    break;
                case BlendMode.Multiply:
                    GL.BlendFunc (BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
                    break;
                case BlendMode.None:
                    GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.Zero);
                    break;
                default:
                case BlendMode.Alpha:
                    GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    break;
            }
        }

        public void ApplyTransform (Transform transform) {
            GL.LoadMatrix (transform.Matrix);
        }

        public void ApplyShader (Shader shader, Transform modelview) {
            Shader.Bind (shader, _currentTextureId, _view.Transform, modelview);
        }

        bool ApplyTexture (Texture texture, bool pixels) {
            int textureid = texture != null ? texture.GlTexId : 0;
            if (_currentTextureId == textureid)
                return false;

            TextureChanges++;
            _currentTextureId = textureid;
            return Texture.Bind (texture, pixels);
        }

        #endregion
    }
}
