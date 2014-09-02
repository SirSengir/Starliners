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

﻿using BLibrary.Util;

namespace BLauncher {
    sealed class ArgumentDefinitions : IArgumentDefinition {

        public bool Reload {
            get;
            private set;
        }

        public string ChannelKey {
            get;
            set;
        }

        public ArgumentDefinitions () {
            Reload = true;
        }

        public bool HandleArgument (string key, string param) {
            switch (key) {
                case "reload":
                    Reload = bool.Parse (param);
                    return true;
                case "channel":
                    ChannelKey = param;
                    return true;
                default:
                    return false;
            }
        }

    }
}

