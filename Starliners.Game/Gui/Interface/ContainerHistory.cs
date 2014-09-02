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
using System.Linq;

namespace Starliners.Gui.Interface {
    sealed class ContainerHistory : Container {

        HistoryTracker _history;

        public ContainerHistory (ushort guiId, Player player)
            : base (guiId) {

            Precedence = "history";
            _history = HistoryTracker.GetForWorld (player.Access);

            UpdateFragment (Constants.FRAGMENT_GUI_HEADER, "log_history");

            Subscribe (_history);
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.HISTORY_INCIDENTS, _history.Incidents.OrderByDescending (p => p.HistoryTick).Take (100).ToList ());
        }

    }
}

