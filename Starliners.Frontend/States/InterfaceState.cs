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

﻿using OpenTK.Input;
using System;
using BLibrary.Graphics;
using BLibrary.Gui;


namespace Starliners.States {

    public abstract class InterfaceState : IInterfaceState {
        #region Properties

        public RenderScene Scene {
            get;
            private set;
        }

        internal ScreenCanvas Canvas {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the state should fade in from the previous state's background.
        /// </summary>
        protected bool FadeIn {
            get;
            set;
        }

        #endregion

        ScreenCanvas _previousCanvas;
        int _previousAlpha = 255;

        public InterfaceState (RenderScene scene) {
            Scene = scene;
            FadeIn = true;
        }

        protected void DrawPrevious (RenderTarget target, RenderStates states) {
            if (_previousAlpha <= 0 || _previousCanvas == null) {
                return;
            }

            _previousCanvas.Alpha = (float)_previousAlpha / 255;
            _previousCanvas.Draw (target, states);
            _previousAlpha -= 3;
        }

        public virtual void OnSwitchedTo (IInterfaceState previous) {

            if (FadeIn) {
                _previousCanvas = ((InterfaceState)previous).Canvas;
            }

            GuiManager.Instance.Wipe ();
            Scene.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs> (OnMouseButtonPressed);
            Scene.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs> (OnMouseButtonReleased);
            Scene.Mouse.Move += new EventHandler<MouseMoveEventArgs> (OnMouseMoved);
            Scene.Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs> (OnMouseWheeled);
            //Scene.Window.MouseLeave += new EventHandler<EventArgs>(OnMouseLeft);
        }

        public virtual void Tick () {
        }

        public abstract void Draw (RenderScene scene, double elapsedTime);

        public virtual void OnExit () {
            Scene.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs> (OnMouseButtonPressed);
            Scene.Mouse.ButtonUp -= new EventHandler<MouseButtonEventArgs> (OnMouseButtonReleased);
            Scene.Mouse.Move -= new EventHandler<MouseMoveEventArgs> (OnMouseMoved);
            Scene.Mouse.WheelChanged -= new EventHandler<MouseWheelEventArgs> (OnMouseWheeled);
            //Scene.Window.MouseLeave -= new EventHandler<EventArgs>(OnMouseLeft);
        }

        #region Keyboard handling

        public virtual void OnTextEntered (char unicode) {
        }

        public virtual void OnKeyPress (Key key) {
        }

        #endregion

        #region Mouse handling

        public void OnMouseButtonPressed (object sender, MouseButtonEventArgs args) {
            if (!GuiManager.Instance.HandleMouseClick (args.X, args.Y, args.Button))
                OnMouseClick (args.X, args.Y, args.Button);
        }

        public virtual void OnMouseClick (int screenX, int screenY, MouseButton button) {
        }

        public void OnMouseButtonReleased (object sender, MouseButtonEventArgs args) {
            if (!GuiManager.Instance.HandleMouseRelease (args.X, args.Y, args.Button))
                OnMouseRelease (args.X, args.Y, args.Button);
        }

        public virtual void OnMouseRelease (int screenX, int screenY, MouseButton button) {
        }

        public void OnMouseLeft (object sender, EventArgs args) {
            Console.Out.WriteLine ("OnMouseLeft");
            if (!GuiManager.Instance.HandleMouseLeave ())
                OnMouseLeave ();
        }

        public virtual void OnMouseLeave () {
        }

        public void OnMouseMoved (object sender, MouseMoveEventArgs args) {
            if (!GuiManager.Instance.HandleMouseMove (args.X, args.Y))
                OnMouseMove (args.X, args.Y);
        }

        public virtual void OnMouseMove (int screenX, int screenY) {
        }

        public void OnMouseWheeled (object sender, MouseWheelEventArgs args) {
            if (!GuiManager.Instance.HandleMouseWheel (args.X, args.Y, args.Delta))
                OnMouseWheel (args.X, args.Y, args.Delta);
        }

        public virtual void OnMouseWheel (int screenX, int screenY, int delta) {
        }

        #endregion
    }
}
