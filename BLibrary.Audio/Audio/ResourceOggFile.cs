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
using csvorbis;
using DragonOgg.MediaPlayer;
using TagLib;

namespace BLibrary.Audio {

    sealed class ResourceOggFile : IOggFileSource {

        public string FileName {
            get;
            private set;
        }

        public VorbisFile VorbisFile {
            get;
            private set;
        }

        public TagLib.File TagLibFile {
            get;
            private set;
        }

        public ResourceOggFile (TagLib.File.IFileAbstraction abstraction) {

            FileName = abstraction.Name;

            try {
                VorbisFile = new VorbisFile (abstraction.ReadStream);
            } catch (Exception ex) {
                throw new OggFileReadException ("Unable to open file for data reading\n" + ex.Message, abstraction.Name);   
            }

            try {
                TagLibFile = TagLib.File.Create (abstraction, "audio/ogg", ReadStyle.Average);
            } catch (TagLib.UnsupportedFormatException ex) {
                throw new OggFileReadException ("Unsupported format (not an ogg?)\n" + ex.Message, abstraction.Name);
            } catch (TagLib.CorruptFileException ex) {
                throw new OggFileCorruptException (ex.Message, abstraction.Name, "Tags");
            }

        }
    }
}

