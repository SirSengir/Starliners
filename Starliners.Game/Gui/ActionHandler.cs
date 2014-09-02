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
using BLibrary.Network;

namespace Starliners.Gui {

    public sealed class ActionHandler : IActionHandler {
        public bool HandleAction (Player player, Container container, string key, Payload args) {
            switch (key) {
                case KeysActions.GUI_OPEN:
                    player.OpenGUI (args.GetValue<ushort> (0), player.Access.RequireIDObject (args.GetValue<ulong> (1)));
                    return true;
                case KeysActions.GUI_OPEN_INCIDENT:
                    IIncident incident = HistoryTracker.GetForWorld (player.Access).RequireIncident<IIncident> (args.GetValue<ulong> (0));
                    player.OpenGUI ((ushort)GuiIds.BattleReport, incident);
                    return true;
                default:
                    return false;
            }
        }
    }
}

