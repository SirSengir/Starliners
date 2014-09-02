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
using System.IO.Packaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BLibrary.Util;

namespace BLibrary.Saves {

    /// <summary>
    /// Encapsulates a save file.
    /// </summary>
    public sealed class SaveGame {
        public FileInfo File { get; private set; }

        public SaveHeader Header { get; private set; }

        public SaveGame (FileInfo file, SaveHeader header) {
            File = file;
            Header = header;
        }

        public static SaveGame AttemptLoad (FileInfo file) {
            try {
                IFormatter formatter = new BinaryFormatter ();
                using (Package packed = Package.Open (file.FullName, FileMode.Open, FileAccess.Read)) {
                    SaveHeader header = (SaveHeader)formatter.Deserialize (ZipUtils.GetStream (packed, "header", formatter));
                    return new SaveGame (file, header);
                }
            } catch (Exception ex) {
                Console.Out.WriteLine ("Skipping invalid save {0}. Reason: {1}", file.FullName, ex.Message);
                return null;
            }
        }

        public override string ToString () {
            return string.Format ("[SaveGame: File={0}, Header={1}]", File.FullName, Header.ToString ());
        }
    }

}
