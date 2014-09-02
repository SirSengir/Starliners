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

namespace Starliners.Network {

    public sealed class PacketRequest : Packet {

        public RequestIds Type {
            get;
            private set;
        }

        public override int Length { get { return HeaderLength + sizeof(byte); } }

        public PacketRequest (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public PacketRequest (RequestIds type)
            : this () {
            Type = type;
        }

        public PacketRequest ()
            : base ((byte)PacketId.Request) {
        }

        public override void ReadData (BinaryReader reader) {
            Type = (RequestIds)reader.ReadByte ();
            System.Console.Out.WriteLine ("Read request: " + Type.ToString ());
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write ((byte)Type);
        }
    }
}
