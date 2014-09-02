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
using BLibrary.Resources;
using BLibrary.Util;
using BLibrary;

namespace Starliners {

    public static class GameAccess {

        /// <summary>
        /// Defines the game folders.
        /// </summary>
        public static FolderManager Folders;
        /// <summary>
        /// Allows access to game definitions simulator side.
        /// </summary>
        public static IGameDefinition Game;
        /// <summary>
        /// Allows access to the crash reporter.
        /// </summary>
        public static ICrashReporter CrashReporter;
        /// <summary>
        /// Allows access to the resources.
        /// </summary>
        public static ResourceRepository Resources;
        /// <summary>
        /// Allows access to the settings.
        /// </summary>
        public static SettingsManager Settings;
        /// <summary>
        /// Holds a reference to the interface.
        /// </summary>
        public static IAccessInterface Interface;
        /// <summary>
        /// Holds a reference to the simulator.
        /// </summary>
        public static IAccessSimulator Simulator;

    }
}

