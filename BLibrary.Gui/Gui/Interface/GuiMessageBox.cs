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
using BLibrary.Gui.Widgets;
using BLibrary.Resources;
using Starliners;

namespace BLibrary.Gui.Interface {
    sealed class GuiMessageBox : GuiLocal {

        #region Constants

        const string WINDOW_KEY = "menu_messagebox";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (384, 160);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        #region Enums

        public enum MessageType {
            None,
            Confirm,
            YesNo
        }

        #endregion

        MessageType _type;
        ITextProvider _title;
        ITextProvider _message;
        Widget.ClickAction[] _actions;

        public GuiMessageBox (ITextProvider title, ITextProvider message, params Widget.ClickAction[] args)
            : this (title, message) {
            if (args.Length <= 0) {
                _type = MessageType.Confirm;
                _actions = new Widget.ClickAction[] {
                    new Widget.ClickPlain (Constants.CONTAINER_KEY_BTN_CLOSE_WINDOW)
                };
            } else if (args.Length == 1) {
                _type = MessageType.Confirm;
                _actions = args;
            } else {
                _type = MessageType.YesNo;
                _actions = args;
            }
        }

        GuiMessageBox (ITextProvider title, ITextProvider message)
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            _title = title;
            _message = message;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (WindowButton.Close, _title);

            Grouping grouped = new Grouping (CornerTopLeft, new Vect2i (Presets.InnerArea.X, Presets.InnerArea.Y - 48));
            grouped.AddWidget (new Label (Vect2i.ZERO, grouped.Size, _message) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });
            AddWidget (grouped);

            Vect2i btnstart = CornerTopLeft + new Vect2i (0, Presets.InnerArea.Y - 40);
            switch (_type) {
                case MessageType.YesNo:
                    int padd = UIProvider.MarginSmall.X;
                    Vect2i btnsize = new Vect2i ((Presets.InnerArea.X - padd) / 2, 40);
                    AddWidget (new Button (btnstart, btnsize, Localization.Instance ["btn_yes"]) {
                        ActionOnClick = _actions [0]
                    });
                    AddWidget (new Button (btnstart + new Vect2i (btnsize.X + UIProvider.MarginSmall.X, 0), btnsize,
                        Localization.Instance ["btn_no"]) {
                        ActionOnClick = _actions [1]
                    });
                    break;
                default:
                    AddWidget (new Button (btnstart, new Vect2i (Presets.InnerArea.X, 40), Localization.Instance ["btn_confirm"]) {
                        ActionOnClick = _actions [0]
                    });
                    break;
            }
        }
    }
}

