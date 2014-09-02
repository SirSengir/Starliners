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
using System.Linq;
using BLibrary.Util;

namespace BLibrary.Saves {
    public static class SaveUtils {

        static FileInfo[] GetSavedFiles (DirectoryInfo directory, string suffix) {
            //DirectoryInfo savedir = GameAccess.Folders [Constants.PATH_SAVES].Location;
            DirectoryInfo savedir = directory;
            if (!savedir.Exists)
                return new FileInfo[0];

            //return savedir.GetFiles ("*?" + Constants.SAVE_SUFFIX).OrderByDescending (f => f.LastWriteTime).ToArray ();
            return savedir.GetFiles ("*?" + suffix).OrderByDescending (f => f.LastWriteTime).ToArray ();
        }

        public static SaveGame[] GetSaves (DirectoryInfo directory, string suffix) {

            FileInfo[] files = GetSavedFiles (directory, suffix);
            List<SaveGame> saves = new List<SaveGame> ();
            foreach (FileInfo file in files) {
                SaveGame load = SaveGame.AttemptLoad (file);
                if (load == null) {
                    continue;
                }
                saves.Add (load);
            }

            return saves.ToArray ();
        }

        public static string CreateSaveName (DirectoryInfo directory, string suffix, string name) {
            if (string.IsNullOrWhiteSpace (name))
                throw new ArgumentNullException ("name");
            name = FileUtils.CreateFileName (name);

            int i = 1;
            string unique = string.Empty;
            FileInfo[] existing = GetSavedFiles (directory, suffix);

            while (string.IsNullOrWhiteSpace (unique)) {
                string test = name + (i > 1 ? "_" + i : "");
                bool exists = false;
                foreach (FileInfo file in existing) {
                    if (file.FullName.Contains (test)) {
                        exists = true;
                        break;
                    }
                }
                if (exists) {
                    i++;
                    continue;
                }
                unique = test;
            }

            return unique;
        }
    }
}

