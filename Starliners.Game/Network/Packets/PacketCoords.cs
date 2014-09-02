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

    public class PacketCoords : Packet {

        public Vect2f Coordinates {
            get;
            private set;
        }

        public override int Length {
            get { return HeaderLength + 2 * sizeof(float); }
        }

        public PacketCoords (PacketId packetid, BinaryReader reader)
            : this (packetid) {
            ReadData (reader);
        }

        public PacketCoords (PacketId packetid, Vect2f coords)
            : this (packetid) {
            Coordinates = coords;
        }

        public PacketCoords (PacketId packetid)
            : base ((byte)packetid) {
        }

        public override void ReadData (BinaryReader reader) {
            Coordinates = new Vect2f (reader.ReadSingle (), reader.ReadSingle ());
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (Coordinates.X);
            writer.Write (Coordinates.Y);
        }

    }
}
