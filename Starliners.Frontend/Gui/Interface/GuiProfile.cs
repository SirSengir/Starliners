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
using BLibrary.Gui;
using BLibrary.Util;
using BLibrary.Resources;
using BLibrary.Gui.Widgets;
using BLibrary;

namespace Starliners.Gui.Interface {
    sealed class GuiProfile : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_profile";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (384, 160);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true);

        const string BUTTON_CONFIRM = "button.quit";

        #endregion

        InputText _input;

        public GuiProfile ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (WindowButton.Close, Localization.Instance ["gui_profile"]);

            Grouping grouped = new Grouping (CornerTopLeft, Presets.InnerArea) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            _input = new InputText (Vect2i.ZERO, new Vect2i (Presets.InnerArea.X, 32), BUTTON_CONFIRM, Localization.Instance ["profile_login"]) {
                CharLimit = 24,
                Entered = Globals.Login,
            };
            grouped.AddWidget (_input);
            grouped.AddWidget (new Button (new Vect2i (0, 32 + UIProvider.MarginSmall.Y), new Vect2i (Presets.InnerArea.X, 40), BUTTON_CONFIRM, Localization.Instance ["btn_ok"]));
        }

        public override bool DoAction (string key, params object[] args) {
            switch (key) {
                case BUTTON_CONFIRM:
                    if (!string.IsNullOrEmpty (_input.Entered)) {
                        Globals.Login = _input.Entered;
                    }
                    Close ();
                    GameAccess.Interface.OpenMainMenu ();
                    return true;
                default:
                    return base.DoAction (key, args);
            }
        }
    }
}

