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
using Starliners.Game;
using BLibrary.Resources;
using BLibrary.Gui.Widgets;
using BLibrary.Gui.Data;
using Starliners.States;
using Starliners;
using BLibrary.Gui.Backgrounds;
using Starliners.Game.Forces;
using Starliners.Gui.Widgets;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui;

namespace Starliners.Gui.Interface {
    sealed class GuiFactionSelect : GuiLocal {

        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 512);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_factionselect", WINDOW_SIZE, Positioning.Centered, true);

        const string BUTTON_BACK = "mainmenu";
        const string BUTTON_FACTION = "faction.select";
        const string BUTTON_LOGIN = "game.login";

        #endregion

        WorldInfo _info;
        string _login;
        int _selectedSlot = -1;

        Button _btnJoin;

        public GuiFactionSelect (WorldInfo info)
            : base (WINDOW_SETTING) {
            IsCloseable = false;
            IsDraggable = false;
            _login = GameAccess.Interface.Launch.Credentials.Login;
            _info = info;

            for (int i = 0; i < _info.Slots.Length; i++) {
                UpdateFragment (string.Format ("player.slot.{0}", i), string.IsNullOrEmpty (_info.Slots [i].PlayerName) ? "<Available>" : _info.Slots [i].PlayerName);
            }
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Vect2i buttonsize = new Vect2i ((Presets.InnerArea.X - UIProvider.Margin.X) / 2, 40);

            AddHeader (WindowButton.None, Localization.Instance ["select_faction"]);
            Grouping grouped = new Grouping (CornerTopLeft, new Vect2i (Presets.InnerArea.X, Presets.InnerArea.Y - buttonsize.Y)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            for (int i = 0; i < _info.Slots.Length; i++) {
                PlayerSlot slot = _info.Slots [i];
                ShipProjector projector = new ShipProjector (string.Format ("{0}4", slot.FleetIcons), slot.Colours.Vessels, slot.Colours.Shields, slot.Serial.GetHashCode ());
                projector.RegisterIcons (SpriteManager.Instance);

                int height = i * buttonsize.Y + 4;

                Grouping ident = new Grouping (new Vect2i (0, height), buttonsize - new Vect2i (0, 4)) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                };
                grouped.AddWidget (ident);

                ident.AddWidget (new IconVessel (new Vect2i (0, 0), new Vect2i (64, 32), projector));
                ident.AddWidget (new Label (new Vect2i (64, 0), new Vect2i (ident.Size.X - 64, ident.Size.Y), slot.Name) {
                    Backgrounds = new BackgroundCollection (new BackgroundSimple (slot.Colours.Empire)),
                    Tinting = NoTinting.INSTANCE,
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                });
                ident.AddWidget (new IconBlazon (new Vect2i (ident.Size.X - 34, 2), new Vect2i (32, 32), slot.Blazon));

                Button button;
                grouped.AddWidget (button = new Button (new Vect2i (buttonsize.X + UIProvider.Margin.X, height), buttonsize, BUTTON_FACTION, new DataReference<string> (this, string.Format ("player.slot.{0}", i)), i));
                button.SetState (ElementState.Disabled, !string.IsNullOrWhiteSpace (slot.PlayerName) && !string.Equals (slot.PlayerName, _login));
            }

            grouped = new Grouping (CornerTopLeft + new Vect2i (0, grouped.Size.Y), new Vect2i (Presets.InnerArea.X, buttonsize.Y)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            grouped.AddWidget (_btnJoin = new Button (new Vect2i (0, 0), buttonsize, BUTTON_LOGIN, Localization.Instance ["faction_select"]));
            grouped.AddWidget (new Button (new Vect2i (buttonsize.X + UIProvider.Margin.X, 0), buttonsize, BUTTON_BACK, Localization.Instance ["btn_nav_mainmenu"]));

            _btnJoin.SetState (ElementState.Disabled, true);
            UpdateSlots ();
        }

        public override bool DoAction (string key, params object[] args) {

            switch (key) {
                case BUTTON_FACTION:
                    int slot = (int)args [0];
                    if (!string.IsNullOrEmpty (_info.Slots [slot].PlayerName)) {
                        return true;
                    }
                    _info.Slots [slot].PlayerName = _login;
                    UpdateSlots ();
                    return true;
                case BUTTON_LOGIN:
                    if (_selectedSlot >= 0) {
                        MapState.Instance.Controller.SelectFaction (_info.Slots [_selectedSlot].Serial);
                        Close ();
                    }
                    return true;
                case BUTTON_BACK:
                    GameAccess.Interface.Retire ();
                    return true;
                default:
                    return false;
            }
        }

        void UpdateSlots () {
            if (_selectedSlot >= 0) {
                _info.Slots [_selectedSlot].PlayerName = string.Empty;
                UpdateFragment (string.Format ("player.slot.{0}", _selectedSlot), string.IsNullOrEmpty (_info.Slots [_selectedSlot].PlayerName) ? "<Available>" : _info.Slots [_selectedSlot].PlayerName);
            }

            _selectedSlot = -1;
            for (int i = 0; i < _info.Slots.Length; i++) {
                UpdateFragment (string.Format ("player.slot.{0}", i), string.IsNullOrEmpty (_info.Slots [i].PlayerName) ? "<Available>" : _info.Slots [i].PlayerName);
                if (string.Equals (_info.Slots [i].PlayerName, _login)) {
                    _selectedSlot = i;
                }
            }

            _btnJoin.SetState (ElementState.Disabled, _selectedSlot < 0);
        }
    }
}

