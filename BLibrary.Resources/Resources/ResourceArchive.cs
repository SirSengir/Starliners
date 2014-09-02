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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace BLibrary.Resources {

    /// <summary>
    /// Represents a resource inside a zip file.
    /// </summary>
    sealed class ResourceArchive : ResourceCollection {
        //FileInfo _file;
        ZipArchive _archive;

        public ResourceArchive (FileInfo file, ZipArchive archive, int weight)
            : base (file.FullName, Version.Parse ("0.0.0.0"), weight) {
            //_file = file;
            _archive = archive;
            SetByMeta ();
        }

        public override IEnumerable<ResourceFile> Search (string pattern) {
            IEnumerable<ResourceFile> result = _archive.Entries.OrderBy (p => p.FullName).Where (p => p.Length > 0 && p.FullName.Replace ('/', '.').Contains (pattern)).Select (p => new ArchiveFile (p));
            return result;
        }

        public override ResourceFile SearchExact (string pattern) {
            return _archive.Entries.OrderBy (p => p.FullName).Where (p => p.FullName.Replace ('/', '.').EndsWith (pattern)).Select (p => new ArchiveFile (p)).FirstOrDefault ();
        }
    }
}
