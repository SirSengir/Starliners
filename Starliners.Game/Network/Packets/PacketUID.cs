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
using Starliners.Game;
using BLibrary;

namespace Starliners.Network {

    public class PacketUID : Packet {
        #region Properties

        public ulong Serial {
            get;
            protected set;
        }

        public override int Length {
            get { return HeaderLength + sizeof(ulong); }
        }

        #endregion

        #region Constructors

        public PacketUID (PacketId packetid, BinaryReader reader)
            : this (packetid) {
            ReadData (reader);
        }

        public PacketUID (PacketId packetid, IdObject obj)
            : this (packetid) {
            if (obj != null) {
                Serial = obj.Serial;
            } else {
                Serial = LibraryConstants.NULL_ID;
            }
        }

        public PacketUID (PacketId packetid, ulong serial)
            : this (packetid) {
            Serial = serial;
        }

        public PacketUID (PacketId packetid)
            : base ((byte)packetid) {
        }

        #endregion

        public override void ReadData (BinaryReader reader) {
            Serial = reader.ReadUInt64 ();
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (Serial);
        }
    }
}
