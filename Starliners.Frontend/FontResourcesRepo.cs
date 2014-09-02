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

using QuickFont;
using System.IO;

namespace Starliners {

    public class FontResourcesRepo : FontResources {

        #region implemented abstract members of FontResources

        public override Stream GetResource (string ident) {
            if (string.IsNullOrEmpty (ident))
                return GameAccess.Resources.SearchResource (_root).OpenRead ();
            else
                return GameAccess.Resources.SearchResource (_prefix + ident).OpenRead ();
        }

        #endregion

        string _root;
        string _prefix;

        public FontResourcesRepo (string root) {
            _root = root;
            _prefix = root.Replace (".qfont", "").Replace (" ", "");
        }
    }
}

