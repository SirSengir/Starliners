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

﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BLibrary.Resources {

    /// <summary>
    /// Represents a resource assembly
    /// </summary>
    sealed class ResourceAssembly : ResourceCollection {
        Assembly _assembly;

        public ResourceAssembly (Assembly assembly, int weight)
            : base (assembly.FullName, assembly.GetName ().Version, weight) {
            _assembly = assembly;
            SetByMeta ();
        }

        public override IEnumerable<ResourceFile> Search (string pattern) {
            IEnumerable<ResourceFile> result = _assembly.GetManifestResourceNames ().OrderByDescending (p => p).Where (p => p.Contains (pattern)).Select (p => new AssemblyFile (_assembly, p));
            return result.OrderBy (p => p.Name);
        }

        public override ResourceFile SearchExact (string pattern) {
            return _assembly.GetManifestResourceNames ().OrderByDescending (p => p).Where (p => p.EndsWith (pattern)).Select (p => new AssemblyFile (_assembly, p)).FirstOrDefault ();
        }

        public override int GetHashCode () {
            return _assembly.GetHashCode ();
        }
    }
}
