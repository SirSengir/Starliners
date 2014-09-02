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
using System.IO;
using System.Collections.Generic;

namespace BLibrary.Util {
    public sealed class FolderManager {

        public ApplicationFolder this [string folder] {
            get {
                return _folders [folder];
            }
        }

        public FileInfo this [string folder, string file] {
            get {
                return new FileInfo (Path.Combine (_folders [folder].Location.FullName, file));
            }
        }

        public string InstancePath {
            get;
            private set;
        }

        Dictionary<string, ApplicationFolder> _folders = new Dictionary<string, ApplicationFolder> ();
        string _appRoot;

        public FolderManager (string appRoot, string instancePath) {
            _appRoot = appRoot;
            InstancePath = instancePath;
        }

        public void DefineFolder (string ident, Environment.SpecialFolder system, string path, bool versioned, bool autocreate) {
            _folders [ident] = new ApplicationFolder (system, _appRoot, InstancePath, path, versioned, autocreate);
        }

        public string GetFilePath (string folder, string file) {
            return Path.Combine (_folders [folder].Location.FullName, file);
        }

        public void SetInstancePath (string instancePath) {
            InstancePath = instancePath;
            foreach (var entry in _folders) {
                entry.Value.SetInstancePath (InstancePath);
            }
        }

        public void SetPaths (string appRoot, string instancePath) {
            _appRoot = appRoot;
            InstancePath = instancePath;
            foreach (var entry in _folders) {
                entry.Value.SetPaths (_appRoot, InstancePath);
            }
        }
    }
}

