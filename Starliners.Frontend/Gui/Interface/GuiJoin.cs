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
using BLibrary.Gui.Data;
using BLibrary.Resources;
using BLibrary.Gui.Widgets;
using System.Net;
using BLibrary.Gui.Backgrounds;

namespace Starliners.Gui.Interface {
    sealed class GuiJoin : GuiLocal {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 512);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("menu_join", WINDOW_SIZE, Positioning.Centered, true);

        const string BUTTON_JOIN = "joingame";
        const string BUTTON_BACK = "mainmenu";

        #endregion

        #region Fields

        InputText _iptAddress;
        InputText _iptPort;

        Button _btnJoin;

        Table _tblServers;

        #endregion

        public GuiJoin ()
            : base (WINDOW_SETTING) {
            IsCloseable = false;
            IsDraggable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Vect2i buttonsize = new Vect2i ((Presets.InnerArea.X - UIProvider.Margin.X) / 2, 40);

            AddHeader (WindowButton.None, Localization.Instance ["join_game"]);

            Grouping grouped = new Grouping (CornerTopLeft, new Vect2i (Presets.InnerArea.X, Presets.InnerArea.Y - buttonsize.Y - UIProvider.Margin.Y)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            Vect2i start = Vect2i.ZERO;
            grouped.AddWidget (new Label (start, new Vect2i (160, 32), string.Format ("{0}:", Localization.Instance ["ip_address"])) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });
            start += new Vect2i (160, 0);
            grouped.AddWidget (_iptAddress = new InputText (start, new Vect2i (grouped.Size.X - 160 - 144 - 128, 32), "input.ipaddress", string.Empty));
            start += new Vect2i (_iptAddress.Size.X, 0);
            grouped.AddWidget (new Label (start, new Vect2i (144, 32), string.Format ("{0}:", Localization.Instance ["ip_port"])) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });
            start += new Vect2i (144, 0);
            grouped.AddWidget (_iptPort = new InputText (start, new Vect2i (128, 32), "input.ipport", string.Empty));
            _iptPort.Entered = 11000.ToString ();

            _tblServers = new Table (new Vect2i (0, 2 * UIProvider.Margin.Y), grouped.Size - new Vect2i (0, 2 * UIProvider.Margin.Y)) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            };
            _tblServers.SetColumnAbsolute (0, 192);
            _tblServers.SetColumnRelative (1, 100);
            _tblServers.SetColumnAbsolute (2, 128);

            grouped.AddWidget (_tblServers);

            grouped = new Grouping (CornerTopLeft + new Vect2i (0, grouped.Size.Y + UIProvider.Margin.Y), new Vect2i (Presets.InnerArea.X, buttonsize.Y)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            grouped.AddWidget (_btnJoin = new Button (new Vect2i (0, 0), buttonsize, BUTTON_JOIN, Localization.Instance ["join_game"]));
            grouped.AddWidget (new Button (new Vect2i (buttonsize.X + UIProvider.Margin.X, 0), buttonsize, BUTTON_BACK, Localization.Instance ["btn_nav_mainmenu"]));

        }

        public override void Update () {
            base.Update ();
            IPAddress address;
            int port;
            _btnJoin.SetState (ElementState.Disabled, string.IsNullOrWhiteSpace (_iptAddress.Entered) || string.IsNullOrWhiteSpace (_iptPort.Entered) || !IPAddress.TryParse (_iptAddress.Entered, out address)
            || !int.TryParse (_iptPort.Entered, out port));
        }

        protected override void Refresh () {
            base.Refresh ();
            _tblServers.Reset (new PopulatorServerTable (_iptAddress, _iptPort));
        }

        public override bool DoAction (string key, params object[] args) {

            switch (key) {
                case BUTTON_BACK:
                    GuiManager.Instance.CloseGuiAll ();
                    GameAccess.Interface.OpenMainMenu ();
                    return true;
                case BUTTON_JOIN:
                    GuiManager.Instance.CloseGuiAll ();
                    GameAccess.Interface.JoinGame (IPAddress.Parse (_iptAddress.Entered), int.Parse (_iptPort.Entered));
                    return true;
                default:
                    return false;
            }
        }

    }
}

