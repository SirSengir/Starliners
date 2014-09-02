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
using Starliners.Network;
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Gui;
using OpenTK.Input;
using Starliners.Map;
using Starliners.Game;

namespace Starliners.States {

    sealed class MapState : InterfaceState {
        #region Constants

        const float SCROLL_DEFAULT_KEY = 0.16f;

        #endregion

        public static MapState Instance {
            get;
            private set;
        }

        /// <summary>
        /// The client side controller to communicate with the server.
        /// </summary>
        public NetInterfaceClient Controller {
            get;
            set;
        }

        public MapInteractive Map {
            get;
            private set;
        }

        internal Vect2f MapScale {
            get { return Scene.Size / Map.View.Size; }
        }

        public MapState (RenderScene scene)
            : base (scene) {
            Instance = this;
            FadeIn = false;
        }

        public void InitMap () {
            WorldInterface.Instance = new WorldInterface ();
            Map = new MapInteractive (WorldInterface.Instance);
        }

        public override void OnSwitchedTo (IInterfaceState previous) {
            base.OnSwitchedTo (previous);
            Controller.RequestAction (RequestIds.Hud);
        }

        public override void OnExit () {
            base.OnExit ();
            if (WorldInterface.Instance != null) {
                WorldInterface.Instance.Halt ();
                WorldInterface.Instance = null;
            }
        }

        public override void Tick () {
            base.Tick ();
            WorldInterface.Instance.MockTick ();

            Vect2f movement = GetLocationChange ();
            if (GameAccess.Interface.ThePlayer.Move (movement)) {
                GameAccess.Interface.Controller.MovedPlayer (movement, GameAccess.Interface.ThePlayer.Location);
                Map.Control.IsUpdated = true;
            }
        }

        public override void Draw (RenderScene scene, double elapsedTime) {
            WorldInterface.Instance.OnFrameStart (elapsedTime);
            Map.Draw (scene, RenderStates.DEFAULT);
            DrawPrevious (scene, RenderStates.DEFAULT);
            GuiManager.Instance.Draw (scene, RenderStates.DEFAULT);
        }

        public override void OnKeyPress (Key key) {
            Map.HandleKeyPress (key);
        }

        bool IsMovementKey (Key key) {
            switch (key) {
                case Key.W:
                case Key.A:
                case Key.S:
                case Key.D:
                    return true;
                default:
                    return false;
            }
        }

        Vect2f GetLocationChange () {
            if (KeyboardHandler.Instance.IsLocked) {
                return Vect2f.ZERO;
            }

            float x = 0, y = 0;
            if (KeyboardHandler.Instance.IsKeyHeld (Key.W)) {
                y += -SCROLL_DEFAULT_KEY;
            }
            if (KeyboardHandler.Instance.IsKeyHeld (Key.A)) {
                x -= SCROLL_DEFAULT_KEY;
            }
            if (KeyboardHandler.Instance.IsKeyHeld (Key.D)) {
                x += SCROLL_DEFAULT_KEY;
            }
            if (KeyboardHandler.Instance.IsKeyHeld (Key.S)) {
                y += SCROLL_DEFAULT_KEY;
            }

            Vect2f change = new Vect2f (x, y);
            if (!KeyboardHandler.Instance.IsKeyHeld (Key.ShiftLeft) && !KeyboardHandler.Instance.IsKeyHeld (Key.ShiftLeft)) {
                change *= 2f;
            }
            return change;
        }

        #region Mouse handling

        public override void OnMouseClick (int screenX, int screenY, MouseButton button) {
            Map.OnMouseClick (screenX, screenY, button);
        }

        public override void OnMouseRelease (int screenX, int screenY, MouseButton button) {
            Map.HandleMouseRelease (screenX, screenY, button);
        }

        public override void OnMouseLeave () {
            Map.HandleMouseLeft ();
        }

        public override void OnMouseMove (int screenX, int screenY) {
            Map.HandleMouseMove (screenX, screenY);
        }

        public override void OnMouseWheel (int screenX, int screenY, int delta) {
            Map.HandleMouseWheel (screenX, screenY, delta);
        }

        #endregion

    }
}

