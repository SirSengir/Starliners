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

﻿using System.IO;
using System.IO.Compression;

namespace BLibrary.Resources {

    /// <summary>
    /// Represents a resource file packaged in a zip.
    /// </summary>
    sealed class ArchiveFile : ResourceFile {

        readonly byte[] _buffer;

        public ArchiveFile (ZipArchiveEntry entry)
            : base (entry.FullName) {
            _buffer = new byte[entry.Length];
            entry.Open ().Read (_buffer, 0, (int)entry.Length);
        }

        public override Stream OpenRead () {
            return new MemoryStream (_buffer);
        }

        protected override string GetResourceIdent (string fullname) {
            return fullname.Replace ('/', '.');
        }

    }
}
