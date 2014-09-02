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
using BLibrary.Gui.Data;
using Starliners.Game;
using Starliners.Gui;
using System.Linq;

namespace Starliners.Gui.Interface {
    sealed class ContainerHud : Container {
        Player _player;

        public ContainerHud (Player player)
            : base ((ushort)GuiIds.Hud) {

            Precedence = KeysPrecedents.HUD;
            Tags.Add (TAG_MENUBAR);
            _player = player;

            UpdateFragment (KeysFragments.PLAYER_NAME, _player.Name);
            Subscribe (_player.Bookkeeping);
            Subscribe (_player.HighScore);
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.PLAYER_FUNDS, _player.Bookkeeping.Funds);
            UpdateFragment (KeysFragments.PLAYER_FUNDS_TRANSACTIONS, _player.Bookkeeping.Records.Where (p => p.Signal && p.TimeStamp > _player.Access.Clock.Ticks - GameClock.TICKS_PER_ROTATION).ToList ());
            UpdateFragment (KeysFragments.PLAYER_SCORE, _player.HighScore.Score);
            UpdateFragment (KeysFragments.PLAYER_SCORE_TRANSACTIONS, _player.HighScore.Records.Where (p => p.TimeStamp > _player.Access.Clock.Ticks - GameClock.TICKS_PER_ROTATION).ToList ());
        }

    }
}

