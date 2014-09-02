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
using BLibrary.Gui.Backgrounds;
using System.Collections.Generic;
using Starliners.Game;

namespace Starliners.Gui.Interface {
    sealed class GuiHistory : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 640);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_history", WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        Table _tblHistory;

        public GuiHistory (int containerId)
            : base (WINDOW_SETTING, containerId) {
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (DEFAULT_BUTTONS, new DataReference<string> (this, "gui.header"));

            AddWidget (_tblHistory = new Table (CornerTopLeft, Presets.InnerArea) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });

        }

        protected override void Refresh () {
            base.Refresh ();
            _tblHistory.Reset (new PopulatorHistoryTable (new DataReference<List<IIncident>> (this, KeysFragments.HISTORY_INCIDENTS)));
        }
    }
}

