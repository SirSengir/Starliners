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
using Starliners.Network;
using System.Net;

namespace Starliners {

    public interface IAccessInterface {

        ServerCache ServerCache { get; }

        /// <summary>
        /// Defines settings for launching a server or game.
        /// </summary>
        LaunchDefinition Launch { get; }

        GameConsole GameConsole { get; }

        object GetGuiElement (Container container);

        object GetTooltip (ushort id, params object[] args);

        IWorldAccess Local { get; }

        NetInterfaceClient Controller { get; }

        bool IsInGame { get; }

        Player ThePlayer { get; }

        Entity HoveredEntity { get; }

        Vect2i WindowSize { get; }

        Vect2i MousePosition { get; }

        float FramesPerSecond { get; set; }

        Dictionary<string, int> GLOperationsPerFrame { get; }

        Thread ThreadMachina { get; set; }

        Thread ThreadNetworking { get; set; }

        bool IsConnected { get; }

        void SwitchTo (object state);

        void OpenMainMenu ();

        void CreateGame (MetaContainer parameters, IScenarioProvider scenario);

        void CreateGame (SaveGame save);

        void JoinGame (IPAddress address, int port);

        void Work ();

        void Retire ();

        void Close ();

        void ConnectTo (IPEndPoint endpoint);

        void HandleNetworking ();

        void CenterView (Vect2f worldCoords);

        void ChangeResolution (Vect2i resolution, bool fullscreen);

        ControlState KeyboardState { get; }

        void BindSecondaryContext ();

        void UnbindSecondaryContext ();
    }
}
