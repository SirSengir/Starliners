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

﻿using System.Collections.Generic;
using System.Threading;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary.Network;
using BLibrary;
using BLibrary.Gui.Data;
using Starliners.Game;


namespace Starliners {

    public interface IAccessSimulator {

        GameConsole GameConsole { get; }

        Container GetGuiContainer (ushort id, Player player, params object[] args);

        bool HandleGuiAction (Player player, Container container, string key, Payload args);

        Thread ThreadMachina { get; set; }

        Thread ThreadNetworking { get; set; }

        bool ShouldStop { get; set; }

        bool AcceptsClients { get; }

        void SetWorldParameters (MetaContainer parameters, IScenarioProvider scenario);

        void SetSaveToLoad (SaveGame save);

        void Work ();
    }
}
