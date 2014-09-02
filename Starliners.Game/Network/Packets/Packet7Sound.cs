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
using BLibrary.Network;
using BLibrary.Util;

namespace Starliners.Network {

    public sealed class Packet7Sound : Packet {

        public string Sound {
            get;
            private set;
        }

        public bool IsLocated {
            get;
            private set;
        }

        public Vect2f Coordinates {
            get;
            private set;
        }

        public override int Length {
            get { return HeaderLength + 2 * sizeof(float) + sizeof(bool) + System.Text.ASCIIEncoding.Unicode.GetByteCount (Sound); }
        }

        public Packet7Sound (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet7Sound (string sound)
            : this () {
            Sound = sound;
            IsLocated = false;
        }

        public Packet7Sound (string sound, Vect2f coordinates)
            : this () {
            Sound = sound;
            Coordinates = coordinates;
            IsLocated = true;
        }

        public Packet7Sound ()
            : base ((byte)PacketId.Sound) {
        }

        public override void ReadData (BinaryReader reader) {
            Sound = reader.ReadString ();
            IsLocated = reader.ReadBoolean ();
            Coordinates = new Vect2f (reader.ReadSingle (), reader.ReadSingle ());
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (Sound);
            writer.Write (IsLocated);
            writer.Write (Coordinates.X);
            writer.Write (Coordinates.Y);
        }

    }
}
