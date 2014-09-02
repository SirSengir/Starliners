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
using BLibrary.Util;
using BLibrary.Resources;
using BLibrary.Gui.Widgets;
using BLibrary.Gui;
using Starliners.Gui;

namespace Starliners.Gui.Interface {
    sealed class GuiChatline : GuiRemote {

        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (640, 32);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_chatline", WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        InputText _ipChat;

        public GuiChatline (int containerId)
            : base (WINDOW_SETTING, containerId) {
        }

        protected override void Regenerate () {
            base.Regenerate ();

            AddHeader (DEFAULT_BUTTONS, Localization.Instance ["chat"]);

            Grouping grouped = new Grouping (CornerTopLeft, Presets.InnerArea) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            grouped.AddWidget (_ipChat = new InputText (Vect2i.ZERO, new Vect2i (Presets.InnerArea.X - 128, 32), KeysActions.CHAT_SEND, "<Chat>"));
            grouped.AddWidget (new Button (new Vect2i (_ipChat.Size.X + 16, 0), new Vect2i (96, 32), Localization.Instance ["btn_send"]) {
                ActionOnClick = new InputText.InputAction (KeysActions.CHAT_SEND, _ipChat)
            });

            _ipChat.EnableActive ();
        }

    }
}

