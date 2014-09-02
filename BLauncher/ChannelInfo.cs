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

﻿using BLibrary.Json;

namespace BLauncher {
    sealed class ChannelInfo {

        public string Key {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        public string Description {
            get;
            private set;
        }

        public string AppRoot {
            get;
            private set;
        }

        public string InstancePath {
            get;
            private set;
        }

        public string ExeFile {
            get;
            private set;
        }

        public ChannelInfo (string key, string name, string appRoot, string instancePath, string exe) {
            Key = key;
            Name = name;
            AppRoot = appRoot;
            InstancePath = instancePath;
            ExeFile = exe;
        }

        public ChannelInfo (JsonObject json) {
            Key = json ["key"].GetValue<string> ();
            Name = json ["name"].GetValue<string> ();
            Description = json.ContainsKey ("description") ? json ["description"].GetValue<string> () : string.Empty;
            AppRoot = json ["root"].GetValue<string> ();
            InstancePath = json ["instance"].GetValue<string> ();
            ExeFile = json ["exe"].GetValue<string> ();
        }
    }
}

