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
using Starliners;

namespace BLibrary.Resources {

    /// <summary>
    /// Represents a resource file on the harddrive.
    /// </summary>
    class SystemFile : ResourceFile {
        FileInfo _info;

        public SystemFile (FileInfo info)
            : base (info.FullName) {
            _info = info;
        }

        public override Stream OpenRead () {
            return _info.OpenRead ();
        }

        protected override string GetResourceIdent (string fullname) {
            return (Constants.PATH_RESOURCES + GameAccess.Folders [Constants.PATH_RESOURCES].ExtractSubPath (fullname)).Replace ('/', '.');
        }
    }
}
