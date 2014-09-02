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

using System;
using BLibrary.Resources;

namespace BLibrary.Audio {

    public class AudioResourceFile : TagLib.File.IFileAbstraction {

        ResourceFile _resource;

        public AudioResourceFile (ResourceFile resource) {
            _resource = resource;
        }

        #region IFileAbstraction implementation

        public string Name {
            get {
                return _resource.Ident;
            }
        }

        public void CloseStream (System.IO.Stream stream) {
            if (stream == null)
                throw new ArgumentNullException ("stream");

            stream.Close ();
        }

        public System.IO.Stream ReadStream {
            get {
                return _resource.OpenRead ();
            }
        }

        public System.IO.Stream WriteStream {
            get {
                throw new NotImplementedException ();
            }
        }

        #endregion
    }
}

