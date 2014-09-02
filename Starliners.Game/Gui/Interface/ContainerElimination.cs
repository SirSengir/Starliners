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
using Starliners.Game;
using BLibrary.Gui.Data;

namespace Starliners.Gui.Interface {
    sealed class ContainerElimination : Container {

        Player _player;

        public ContainerElimination (ushort guiId, Player player)
            : base (guiId) {

            Precedence = "elimination";
            _player = player;
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.PLAYER_SCORE, _player.HighScore);
            UpdateFragment (KeysFragments.FACTION_SERIAL, _player.MainFaction.Serial);
            UpdateFragment (KeysFragments.FACTION_STATISTICS, _player.MainFaction.Statistics);
        }
    }
}

