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
using BLibrary.Serialization;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary;
using Starliners.Game;
using Starliners.Network;

namespace Starliners {

    public interface IGameDefinition {

        /// <summary>
        /// Gets a string identifying the name of the data assembly.
        /// </summary>
        /// <value>The data assembly name.</value>
        string DataAssembly { get; }

        /// <summary>
        /// Geta string identifying the name of the resource assembly
        /// </summary>
        /// <value>The resource assembly name.</value>
        string ResourceAssembly { get; }

        /// <summary>
        /// Gets the minimum map zoom allowed.
        /// </summary>
        /// <value>The minimum zoom.</value>
        int MinZoom { get; }

        /// <summary>
        /// Get the maximum map zoom allowed.
        /// </summary>
        /// <value>The max zoom.</value>
        int MaxZoom { get; }

        /// <summary>
        /// Gets the default map zoom.
        /// </summary>
        /// <value>The default zoom.</value>
        int DefaultZoom { get; }

        ushort IdGuiPopup { get; }

        /// <summary>
        /// Gets the list of scenarios available.
        /// </summary>
        /// <value>The scenarios.</value>
        IReadOnlyList<IScenarioProvider> Scenarios { get; }

        /// <summary>
        /// Gets the list of available options when creating a game.
        /// </summary>
        /// <value>The setup options.</value>
        IReadOnlyList<ParameterOptions> SetupOptions { get; }

        /// <summary>
        /// Gets a list of sounds to register for use in the game.
        /// </summary>
        /// <value>The sounds.</value>
        IReadOnlyList<SoundDefinition> Sounds { get; }

        /// <summary>
        /// Gets a list of additional log levels specific to the game.
        /// </summary>
        /// <value>The log levels.</value>
        IReadOnlyList<LogLevel> LogLevels { get; }

        /// <summary>
        /// Creates a new set of default parameters for a world.
        /// </summary>
        /// <returns>The default parameters.</returns>
        MetaContainer CreateDefaultParameters ();

        /// <summary>
        /// Gets a mapper to use when deserializing the given save game.
        /// </summary>
        /// <returns>The save mapper.</returns>
        /// <param name="header">Header of the save game to be loaded.</param>
        SaveMapper GetSaveMapper (SaveHeader header, GameConsole console);

        void HandleRequest (NetInterfaceServer netInterface, byte request);

        bool HandleKeyboardShortcut (KeysU key);

        void Tick (IWorldAccess access, TickType ticks);

        void InitWorld (IWorldAccess access);

        void OnWorldLoad (IWorldAccess access);

    }
}
