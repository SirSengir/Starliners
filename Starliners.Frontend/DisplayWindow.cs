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

﻿using BLibrary.Resources;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using OpenTK.Input;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Audio;
using Starliners.Map;
using Starliners.States;

namespace Starliners {

    /// <summary>
    /// This is the implementation of OpenTK's GameWindow.
    /// </summary>
    internal sealed class DisplayWindow : GameWindow, IDisplayWindow {
        public INativeWindow SecondaryWindow {
            get;
            set;
        }

        IInterfaceState _uiState;
        RenderScene _scene;
        IInterfaceDefinition _interfaceDefinition;
        IGraphicsContext _secondaryContext;
        IInterfaceState _nextState;

        public DisplayWindow (RenderScene scene, IInterfaceDefinition uiProvider, int width, int height, bool fullscreen)
            : base (width, height, GraphicsMode.Default, "Bees'n'Trees", fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default, DisplayDevice.Default, 1, 0, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible) {
            _scene = scene;
            _interfaceDefinition = uiProvider;
        }

        #region OnLoad

        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad (EventArgs e) {

            base.OnLoad (e);
            Title = "Starliners";

            ResourceFile icon = GameAccess.Resources.SearchResource ("Icon.ico");
            using (Stream stream = icon.OpenRead ()) {
                Icon = new System.Drawing.Icon (stream);
            }

            KeyboardHandler.Instance = new KeyboardHandler (this);
            KeyboardHandler.Instance.TextEntered += (KeyboardHandler sender, CustomEventArgs<char> args) => {
                _uiState.OnTextEntered (args.Argument);
            };
            KeyboardHandler.Instance.KeyPressed += (KeyboardHandler sender, CustomEventArgs<Key> args) => {
                _uiState.OnKeyPress (args.Argument);
            };
            _secondaryContext = new GraphicsContext (GraphicsMode.Default, SecondaryWindow.WindowInfo);

            MakeCurrent ();
        }

        #endregion

        public void SwitchTo (object state) {
            _nextState = (IInterfaceState)state;
        }

        #region OnResize

        /// <summary>
        /// Respond to resize events here.
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize (EventArgs e) {
            GL.Viewport (0, 0, Width, Height);

            GL.MatrixMode (MatrixMode.Projection);
            GL.LoadIdentity ();
            GL.Ortho (-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

            _scene.OnResized (this);
        }

        #endregion

        #region OnUpdateFrame

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame (FrameEventArgs e) {
            GameAccess.Interface.HandleNetworking ();
            if (_uiState != null) {
                _uiState.Tick ();
            }

        }

        #endregion

        #region OnRenderFrame

        int fpsThrottle = 0;
        double elapseFrame = 0;

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame (FrameEventArgs e) {

            if (_uiState == null) {
                SwitchTo (new StartupState (_scene, _interfaceDefinition));
            }
            if (_nextState != null) {
                if (_uiState != null) {
                    _uiState.OnExit ();
                }

                IInterfaceState previous = _uiState;
                _uiState = _nextState;
                _uiState.OnSwitchedTo (previous);
                _nextState = null;
            }

            fpsThrottle++;
            elapseFrame += e.Time;
            if (fpsThrottle >= 20) {
                // Compute fps.
                GameAccess.Interface.FramesPerSecond = (float)(20f / elapseFrame);
                elapseFrame = 0;
                fpsThrottle = 0;
            }
            GameAccess.Interface.GLOperationsPerFrame ["DrawCalls"] = RenderTarget.DrawCalls;
            GameAccess.Interface.GLOperationsPerFrame ["TextureChanges"] = RenderTarget.TextureChanges;
            GameAccess.Interface.GLOperationsPerFrame ["VBVertUpdates"] = RenderTarget.VBVertUpdates;
            GameAccess.Interface.GLOperationsPerFrame ["PushedGLStates"] = RenderTarget.PushedGLStates;
            GameAccess.Interface.GLOperationsPerFrame ["ViewChanges"] = RenderTarget.ViewChanges;
            GameAccess.Interface.GLOperationsPerFrame ["FBOChanges"] = RenderTarget.FBOChanges;

            RenderTarget.DrawCalls = 0;
            RenderTarget.TextureChanges = 0;
            RenderTarget.VBVertUpdates = 0;
            RenderTarget.PushedGLStates = 0;
            RenderTarget.ViewChanges = 0;
            RenderTarget.FBOChanges = 0;

            _scene.Clear ();
            if (MapRendering.Instance != null) {
                MapRendering.Instance.OnFrameStart ();
            }
            SpriteManager.Instance.OnFrameStart (e.Time);
            _uiState.Draw (_scene, e.Time);

            SwapBuffers ();

            // Clean up orphaned OpenGL resources.
            GlManager.CleanOrphans ();
        }

        #endregion

        #region OnUnload

        protected override void OnUnload (EventArgs e) {
            base.OnUnload (e);

            SoundManager.Instance.Stop ();
            SoundManager.Instance.Dispose ();

            SpriteManager.Instance.DisposeTextures ();
            if (MapState.Instance != null && MapState.Instance.Map != null) {
                MapState.Instance.Map.Dispose ();
            }
            _interfaceDefinition.Dispose ();

            // Dispose of secondary window and context.
            _secondaryContext.MakeCurrent (null);
            SecondaryWindow.Close ();
            _secondaryContext.Dispose ();

            // Clean up orphans one more time.
            GlManager.CleanOrphans ();
        }

        #endregion

        #region OpenGL Context

        public void BindSecondaryContext () {
            _secondaryContext.MakeCurrent (WindowInfo);
        }

        public void UnbindSecondaryContext () {
            _secondaryContext.MakeCurrent (null);
        }

        #endregion
    }
}
