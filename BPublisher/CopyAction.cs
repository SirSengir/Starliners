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

namespace BPublisher {
    sealed class CopyAction : BuildAction {

        FileInfo _source;
        FileInfo _target;

        public CopyAction (JsonObject json) {
            _source = new FileInfo (json ["source"].GetValue<string> ());
            _target = new FileInfo (json ["target"].GetValue<string> ());
        }

        public override bool Execute (DirectoryInfo root, DirectoryInfo repo) {
            if (!_source.Exists) {
                return false;
            }

            if (!_target.Directory.Exists) {
                _target.Directory.Create ();
            }

            FileInfo copied;
            try {
                copied = _source.CopyTo (_target.FullName, true);
            } catch (Exception ex) {
                Console.Out.WriteLine (ex.Message);
                return false;
            }
            return copied != null ? copied.Exists : false;
        }
    }
}

