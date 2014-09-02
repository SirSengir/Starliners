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
using BLibrary.Gui.Widgets;
using Starliners.Gui.Widgets;

namespace Starliners.Gui.Interface {
    sealed class GuiFleet : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (512, 256);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_fleet", WINDOW_SIZE, Positioning.LowerRight, true);

        static readonly Vect2i BUTTON_SIZE = new Vect2i (64, 64);
        static readonly Vect2i SYMBOL_SIZE = new Vect2i (48, 48);
        static readonly Vect2i SYMBOL_OFFSET = (Vect2i)((BUTTON_SIZE - SYMBOL_SIZE) / 2);
        static readonly Vect2i BUTTON_SPACING = new Vect2i (64, 0);

        #endregion

        public GuiFleet (int containerId)
            : base (WINDOW_SETTING, containerId) {
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (DEFAULT_BUTTONS, new DataReference<string> (this, "gui.header"));

            Vect2i start = CornerTopLeft;
            Grouping frame = new Grouping (start, new Vect2i (Presets.InnerArea.X, 128)) { Backgrounds = UIProvider.Style.CreateInset () };
            frame.AddWidget (new FleetViewer (new Vect2i (2, 2), frame.Size - new Vect2i (4, 4), new DataReference<ulong> (this, KeysFragments.FLEET_SERIAL)));
            AddWidget (frame);

            start += new Vect2i (0, frame.Size.Y + UIProvider.Margin.Y);
            frame = new Grouping (start, new Vect2i (Presets.InnerArea.X, Presets.InnerArea.Y - frame.Size.Y - 2 * UIProvider.Margin.Y)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            frame.AddWidget (new Button (Vect2i.ZERO, BUTTON_SIZE, KeysActions.FLEET_RELOCATE, new IconSymbol (SYMBOL_OFFSET, SYMBOL_SIZE, "pickup")));
            frame.AddWidget (new Button (BUTTON_SPACING, BUTTON_SIZE, KeysActions.FLEET_COMPOSITION, new IconSymbol (SYMBOL_OFFSET, SYMBOL_SIZE, "info")));
            frame.AddWidget (new Button (BUTTON_SPACING * 2, BUTTON_SIZE, KeysActions.FLEET_RENAME, new IconSymbol (SYMBOL_OFFSET, SYMBOL_SIZE, "rename")));
            frame.AddWidget (new Button (BUTTON_SPACING * 3, BUTTON_SIZE, KeysActions.FLEET_DISBAND, new IconSymbol (SYMBOL_OFFSET, SYMBOL_SIZE, "invalid")));
            AddWidget (frame);

        }
    }
}

