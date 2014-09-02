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

namespace BLibrary.Util {

    public sealed class ApplicationFolder {
        #region Properties

        public DirectoryInfo Location {
            get;
            private set;
        }

        #endregion

        Environment.SpecialFolder _folder;
        string _root;
        string _instance;
        string _path;
        bool _versioned;
        bool _autocreate;

        public ApplicationFolder (Environment.SpecialFolder folder, string root, string instance, string path, bool versioned, bool autocreate) {
            _folder = folder;
            _root = root;
            _instance = instance;
            _path = path;
            _versioned = versioned;
            _autocreate = autocreate;

            SetLocation ();
        }

        private void SetLocation () {
            if (_versioned) {
                Location = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (_folder), _root, _instance, _path));
            } else {
                Location = new DirectoryInfo (Path.Combine (Environment.GetFolderPath (_folder), _root, _path));
            }
            CreateIfNeeded ();
        }

        private void CreateIfNeeded () {
            if (_autocreate) {
                if (!Location.Exists) {
                    Location.Create ();
                }
            }
        }

        public string GetFileWithin (string file) {
            return Path.Combine (Location.FullName, file);
        }

        public string ExtractSubPath (string filePath) {
            return filePath.Replace (Location.FullName, "");
        }

        internal void SetInstancePath (string instancePath) {
            _instance = instancePath;
            SetLocation ();
        }

        internal void SetPaths (string appRoot, string instancePath) {
            _root = appRoot;
            _instance = instancePath;
            SetLocation ();
        }
    }
}

