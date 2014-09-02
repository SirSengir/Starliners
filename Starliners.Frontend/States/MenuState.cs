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

﻿using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Gui;
using BLibrary.Gui.Interface;
using Starliners.Gui.Interface;

namespace Starliners.States {

    sealed class MenuState : InterfaceState {
        TextBuffer _buffer;

        #region Constructor

        public MenuState (RenderScene scene)
            : base (scene) {
            Canvas = new FlatCanvas ("Textures/Menus/Main");
        }

        #endregion

        public override void OnSwitchedTo (IInterfaceState previous) {
            base.OnSwitchedTo (previous);

            GuiManager.Instance.OpenGui (new GuiMenu ());
        }

        string _warning = "Proof of Concept! DO NOT REDISTRIBUTE!";

        public override void Draw (RenderScene scene, double elapsedTime) {

            if (Canvas != null) {
                Canvas.Draw (scene, RenderStates.DEFAULT);
            }
            RenderStates states = RenderStates.DEFAULT;

            if (_buffer == null) {
                string version = PlatformUtils.GetEXEVersion ().ToString ();
                string hashstring = GameAccess.Resources.GetIdentHash ();
                _buffer = new TextBuffer (_warning, string.Format ("v{0} ({1})", version, hashstring));
            }

            states.Transform.Translate (8, Scene.Size.Y - 8 - _buffer.LocalBounds.Size.Y);
            _buffer.Draw (scene, states);
            DrawPrevious (scene, RenderStates.DEFAULT);
            GuiManager.Instance.Draw (scene, RenderStates.DEFAULT);
        }
    }
}
