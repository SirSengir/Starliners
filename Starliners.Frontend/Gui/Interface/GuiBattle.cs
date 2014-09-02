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
using System.Collections.Generic;
using Starliners.Gui.Battlefield;
using BLibrary.Gui;
using BLibrary.Gui.Backgrounds;
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using Starliners.Game;
using Starliners.Game.Forces;
using Starliners.Gui.Widgets;

namespace Starliners.Gui.Interface {
    sealed class GuiBattle : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 640);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_battle", WINDOW_SIZE, Positioning.Centered, true);

        static readonly Vect2i VIEWER_SIZE = new Vect2i (960, 384);

        #endregion

        Table _tblAttackers;
        Table _tblDefenders;


        public GuiBattle (int containerId)
            : base (WINDOW_SETTING, containerId) {
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (DEFAULT_BUTTONS, new DataReference<string> (this, "gui.header"));

            Vect2i start = CornerTopLeft;
            AddWidget (new BattlefieldViewer (start, VIEWER_SIZE,
                new DataReference<BattleGrid> (this, KeysFragments.BATTLE_GRID_ATTACKER), new DataReference<BattleGrid> (this, KeysFragments.BATTLE_GRID_DEFENDER)));

            Vect2i tablesize = new Vect2i ((Presets.InnerArea.X - UIProvider.Margin.X) / 2, Presets.InnerArea.Y - VIEWER_SIZE.Y - UIProvider.Margin.Y);

            start += new Vect2i (0, VIEWER_SIZE.Y + UIProvider.Margin.Y);
            AddWidget (_tblAttackers = new Table (start, tablesize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });
            _tblAttackers.SetColumnAbsolute (0, 48);
            _tblAttackers.SetColumnRelative (1, 100);
            _tblAttackers.SetColumnAbsolute (2, 160);

            start += new Vect2i (tablesize.X + UIProvider.Margin.X, 0);
            AddWidget (_tblDefenders = new Table (start, tablesize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });
            _tblDefenders.SetColumnAbsolute (0, 48);
            _tblDefenders.SetColumnRelative (1, 100);
            _tblDefenders.SetColumnAbsolute (2, 160);

        }

        protected override void Refresh () {
            base.Refresh ();

            _tblAttackers.Reset (new PopulatorFleetTable (new DataReference<List<ulong>> (this, KeysFragments.BATTLE_FLEETS_ATTACKER)));
            _tblDefenders.Reset (new PopulatorFleetTable (new DataReference<List<ulong>> (this, KeysFragments.BATTLE_FLEETS_DEFENDER)));

        }
    }
}

