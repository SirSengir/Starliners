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
using BLibrary.Json;
using System.IO;
using System.Diagnostics;

namespace BPublisher {
    sealed class LinkAction : BuildAction {

        string _link;

        public LinkAction (JsonObject json) {
            _link = json.ContainsKey ("link") ? json ["link"].GetValue<string> () : "current";
        }

        public override bool Execute (DirectoryInfo root, DirectoryInfo repo) {
            FileInfo linked = new FileInfo (Path.Combine (root.FullName, _link));

            Console.Out.WriteLine ("Linking {0} to {1}.", repo.FullName, linked.FullName);
            Process.Start ("./rm", linked.FullName);
            Process.Start ("./ln", "-s " + repo.FullName + " " + linked.FullName);
            return true;
        }
    }
}

