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

﻿using System;
using System.IO;
using BLibrary.Network;

namespace Starliners.Network {
    public sealed class PacketAck : Packet {

        public enum ResponseCode : byte {
            None,
            Ok,
            NotOk
        }

        public ResponseCode Response {
            get;
            private set;
        }

        public byte Reference {
            get;
            private set;
        }

        public string Note {
            get;
            set;
        }

        public override int Length { get { return HeaderLength + 2 * sizeof(byte) + System.Text.ASCIIEncoding.Unicode.GetByteCount (Note); } }

        #region Constructor

        public PacketAck (PacketId id, BinaryReader reader)
            : this (id) {
            ReadData (reader);
        }

        public PacketAck (PacketId id, ResponseCode response, byte reference)
            : this (id) {
            Response = response;
            Reference = reference;
        }

        public PacketAck (PacketId id)
            : base ((byte)id) {
        }

        #endregion

        public override void ReadData (BinaryReader reader) {
            Response = (ResponseCode)reader.ReadByte ();
            Reference = reader.ReadByte ();
            Note = reader.ReadString ();
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write ((byte)Response);
            writer.Write (Reference);
            writer.Write (Note);
        }
    }
}

