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
using BLibrary.Util;

namespace BLibrary.Graphics {

    public sealed class RenderScene : RenderTarget {

        /// <summary>
        /// Returns the (window) size of this scene.
        /// </summary>
        public override Vect2i Size {
            get { return _size; }
        }

        /// <summary>
        /// Returns OpenTK's GameWindow.
        /// </summary>
        internal IDisplayWindow Window {
            get { return _window; }
            set {
                _window = value;
                Reset (_window);
            }
        }

        /// <summary>
        /// Returns OpenTK's KeyboardDevice.
        /// </summary>
        public KeyboardDevice Keyboard {
            get { return _window.Keyboard; }
        }

        /// <summary>
        /// Returns OpenTK's MouseDevice.
        /// </summary>
        public MouseDevice Mouse {
            get { return _window.Mouse; }
        }

        #region Fields

        Vect2i _size;
        IDisplayWindow _window;

        #endregion

        public RenderScene () {
            FBOId = 0;
        }

        internal void OnResized (IDisplayWindow openTkWindow) {
            _window = openTkWindow;
            Reset (openTkWindow);
        }

        void Reset (IDisplayWindow openTkWindow) {
            _size = new Vect2i (openTkWindow.Width, openTkWindow.Height);
            DefaultView = new View ((Vect2i)(_size / 2), _size);
            View = DefaultView;
        }

        public void SetMouseCursorVisible (bool flag) {
        }

    }
}
