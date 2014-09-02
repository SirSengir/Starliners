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
using OpenTK.Input;
using BLibrary.Util;
using BLibrary.Gui;

namespace Starliners {

    public delegate void TextEnteredEventHandler (KeyboardHandler sender, CustomEventArgs<char> args);
    public delegate void KeyPressEventHandler (KeyboardHandler sender, CustomEventArgs<Key> args);

    /// <summary>
    /// Abstraction for keyboard input.
    /// </summary>
    public class KeyboardHandler {
        public static KeyboardHandler Instance {
            get;
            set;
        }

        public bool IsLocked {
            get { return _lock != null || !_window.Focused; }
        }

        Timer _repeatDelay;
        GameWindow _window;
        char _lastChar;
        object _lock;

        #region Constructor

        public KeyboardHandler (GameWindow window) {
            _repeatDelay = new Timer (150);

            _window = window;
            _window.KeyPress += new EventHandler<KeyPressEventArgs> (OnTextEntered);
            _window.KeyDown += new EventHandler<KeyboardKeyEventArgs> (OnKeyDown);
            _window.Keyboard.KeyRepeat = true;
        }

        #endregion

        public bool IsKeyHeld (Key key) {
            return Keyboard.GetState ().IsKeyDown (key);
        }

        public event TextEnteredEventHandler TextEntered;
        public event KeyPressEventHandler KeyPressed;

        public void OnTextEntered (object sender, KeyPressEventArgs args) {

            if (args.KeyChar.Equals (_lastChar) && _repeatDelay.IsDelayed) {
                return;
            }

            if (!GuiManager.Instance.HandleTextEntered (args.KeyChar)) {
                if (TextEntered != null) {
                    TextEntered (this, new CustomEventArgs<char> (args.KeyChar));
                }
            }

            _lastChar = args.KeyChar;
            _repeatDelay.Reset ();
        }

        public void OnKeyDown (object sender, KeyboardKeyEventArgs args) {
            if (!GuiManager.Instance.HandleKeyPress (args.Key)) {
                if (KeyPressed != null) {
                    KeyPressed (this, new CustomEventArgs<Key> (args.Key));
                }
            }
        }

        public void Lock (object obj) {
            _lock = obj;
        }

        public void Unlock (object obj) {
            if (obj == _lock) {
                _lock = null;
            }
        }
    }
}

