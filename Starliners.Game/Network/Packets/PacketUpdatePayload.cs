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
using Starliners.Game;

namespace Starliners.Network {

    public sealed class PacketUpdatePayload : Packet, IPacketMarked {
        public ulong Serial { get; private set; }

        public UpdateMarker Marker { get; private set; }

        public Payload Payload { get; private set; }

        public override int Length {
            get { return HeaderLength + Payload.Length + sizeof(ulong) + sizeof(int); }
        }

        public PacketUpdatePayload (PacketId packetId, BinaryReader reader)
            : this (packetId) {
            ReadData (reader);
        }

        public PacketUpdatePayload (PacketId packetId, IdObject idobject, UpdateMarker marker, params object[] args)
            : this (packetId) {

            Marker = marker;
            if (idobject != null) {
                Serial = idobject.Serial;
            } else {
                throw new ArgumentException ("Cannot update null object.");
            }
            Payload = new Payload (idobject.Access, args);
        }

        public PacketUpdatePayload (PacketId packetId)
            : base ((byte)packetId) {
        }

        public override void ReadData (BinaryReader reader) {
            Serial = reader.ReadUInt64 ();
            Marker = (UpdateMarker)reader.ReadInt32 ();
            Payload = new Payload (reader);
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (Serial);
            writer.Write ((int)Marker);
            Payload.WriteData (writer);
        }
    }
}
