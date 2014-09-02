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

﻿using BLibrary.Gui.Data;
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using Starliners;
using Starliners.States;

namespace BLibrary.Gui.Interface {

    public sealed class GuiPrompt : GuiWindow {
        #region Properties

        const string WINDOW_KEY = "ig_prompt";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (256, 66);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true) {
            Group = ScreenGroup.Input
        };

        const string BUTTON_CONFIRM = "button.confirm";

        #endregion

        protected override bool CanDraw {
            get {
                return true;
            }
        }

        string _key;
        WidgetContainer _proxied;

        ITextProvider _header;
        ITextProvider _button;
        ITextProvider _placeholder;

        InputText _input;

        public GuiPrompt (WidgetContainer proxied, string key, ITextProvider header, ITextProvider button, ITextProvider placeholder)
            : base (WINDOW_SETTING) {

            _proxied = proxied;
            _key = key;
            _header = header;
            _button = button;
            _placeholder = placeholder;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            AddHeader (WindowButton.None, _header);

            _input = new InputText (CornerTopLeft, new Vect2i (Presets.InnerArea.X, 24), "input.text", _placeholder.ToString ()) { CharLimit = 24 };
            AddWidget (_input);
            AddWidget (new Button (CornerTopLeft + new Vect2i (0, 32), new Vect2i (Presets.InnerArea.X, 24), BUTTON_CONFIRM, _button.ToString ()));
        }

        public override bool DoAction (string key, params object[] args) {
            if (BUTTON_CONFIRM.Equals (key)) {
                _proxied.DoAction (_key, _input.Entered);
                Close ();
                return true;
            }
            return false;
        }
    }
}
