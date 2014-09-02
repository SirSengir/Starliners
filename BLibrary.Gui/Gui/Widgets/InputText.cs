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
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using Starliners;
using BLibrary.Audio;

namespace BLibrary.Gui.Widgets {

    public sealed class InputText : Widget {
        #region Classes

        public class InputAction : ClickAction {
            string _key;
            InputText _input;

            public InputAction (string key, InputText input) {
                _key = key;
                _input = input;
            }

            public override void DoAction (WidgetContainer window, ControlState control) {
                if (!string.IsNullOrWhiteSpace (_input.Entered)) {
                    window.DoAction (_key, _input.Entered);
                }
            }
        }

        #endregion

        #region Properties

        public int CharLimit {
            get;
            set;
        }

        public ClickAction ActionOnEnterOrEscape {
            get;
            set;
        }

        public int PaddingX {
            get;
            set;
        }

        public string Entered {
            get;
            set;
        }

        bool IsActive {
            get;
            set;
        }

        #endregion

        #region Fields

        Rectangle _inputCursor;
        TextBuffer _buffer;
        string _placeholder;

        #endregion

        public InputText (Vect2i position, Vect2i size, string key, string placeholder)
            : this (position, size, placeholder) {
            ActionOnEnterOrEscape = new InputAction (key, this);
        }

        public InputText (Vect2i position, Vect2i size, string placeholder)
            : base (position, size) {
            PaddingX = 8;

            _placeholder = placeholder;
            _buffer = new TextBuffer (_placeholder);
            _buffer.Box = Size - new Vect2i (PaddingX * 2, 0);
            _buffer.VAlign = Alignment.Center;

            Backgrounds = UIProvider.Style.InputStyle.CreateBackgrounds ();
            CharLimit = 512;
        }

        public override void Draw (RenderTarget target, RenderStates states) {

            states.Transform.Translate (PositionRelative);
            Backgrounds.SetActive (IsActive ? ElementState.Active : ElementState.None);
            DrawBackground (target, states);

            // Placeholder
            if (!IsActive && string.IsNullOrEmpty (Entered)) {

                states.Transform.Translate (PaddingX, 0);
                _buffer.Text = _placeholder;
                _buffer.Draw (target, states);

                // Display entered text
            } else {

                if (!string.IsNullOrEmpty (Entered)) {
                    states.Transform.Translate (PaddingX, 0);
                    _buffer.Text = Entered;
                    _buffer.Draw (target, states);
                }

                // Add cursor if active
                if (IsActive) {
                    if (_inputCursor == null) {
                        _inputCursor = new Rectangle (new Vect2i (2, _buffer.LocalBounds.Size.Y - 2));
                    }
                    _inputCursor.Alpha = MathUtils.AsAmplitude (SpriteManager.Instance.Metronom1);
                    states.Transform.Translate (string.IsNullOrEmpty (Entered) ? PaddingX : _buffer.LocalBounds.Size.X, (Size.Y - _buffer.LocalBounds.Size.Y) / 2);
                    target.Draw (_inputCursor, states);
                }
            }
        }

        public override bool HandleTextEntered (char unicode) {
            if (!IsActive) {
                return false;
            }

            if (CharLimit > 0 && (Entered == null || Entered.Length < CharLimit)
                && (' '.Equals (unicode) || _buffer.Fonts.CanRenderChar (unicode))) {
                Entered += unicode;
            } else {
                SoundManager.Instance.Play (SoundKeys.FAIL);
            }
            return true;
        }

        public override bool HandleKeyPress (OpenTK.Input.Key key) {
            if (!IsActive) {
                return false;
            }

            KeyboardState state = Keyboard.GetState ();
            if (state.IsKeyDown (OpenTK.Input.Key.BackSpace)
                || state.IsKeyDown (OpenTK.Input.Key.Delete)) {

                if (Entered != null && Entered.Length > 0)
                    Entered = Entered.Remove (Entered.Length - 1);

                return true;

            } else if (state.IsKeyDown (OpenTK.Input.Key.Enter)) {

                if (ActionOnEnterOrEscape != null) {
                    ActionOnEnterOrEscape.DoAction (Window, ControlState.None);
                    Reset ();
                }
                DisableActive ();
                return true;

            } else if (state.IsKeyDown (OpenTK.Input.Key.Escape)) {

                if (ActionOnEnterOrEscape != null) {
                    ActionOnEnterOrEscape.DoAction (Window, ControlState.None);
                    Reset ();
                }
                DisableActive ();

                return true;
            }

            return false;
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            if (IntersectsWith (coordinates)) {
                EnableActive ();
                return false;
            }

            DisableActive ();
            return false;
        }

        void Reset () {
            Entered = null;
        }

        public void EnableActive () {
            IsActive = true;
            GuiManager.Instance.KeypressBlocked = true;
            KeyboardHandler.Instance.Lock (this);
        }

        void DisableActive () {
            IsActive = false;
            KeyboardHandler.Instance.Unlock (this);
        }
    }
}
