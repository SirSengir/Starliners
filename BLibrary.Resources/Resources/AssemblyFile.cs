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
using System.Reflection;

namespace BLibrary.Resources {
    /// <summary>
    /// Represents a resource file embedded in an assembly.
    /// </summary>
    sealed class AssemblyFile : ResourceFile {

        readonly Assembly _assembly;

        public AssemblyFile (Assembly assembly, string name)
            : base (name) {
            _assembly = assembly;
        }

        public override Stream OpenRead () {
            return _assembly.GetManifestResourceStream (Name);
        }

        protected override string GetResourceIdent (string fullname) {
            return fullname.Remove (0, fullname.IndexOf ('.') + 1);
        }

    }
}
