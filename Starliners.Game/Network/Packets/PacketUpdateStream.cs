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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Starliners.Game;
using BLibrary;
using BLibrary.Network;

namespace Starliners.Network {

    public class PacketUpdateStream : PacketSerialized, IPacketMarked {
        public ulong Serial {
            get;
            private set;
        }

        public UpdateMarker Marker {
            get;
            private set;
        }

        public override int Length {
            get {
                return base.Length + sizeof(ulong) + sizeof(int);
            }
        }

        #region Constructor

        public PacketUpdateStream (PacketId packetId, BinaryReader reader)
            : base (packetId) {
            ReadData (reader);
        }

        public PacketUpdateStream (PacketId packetId, IdObject idobject, UpdateMarker marker, ISerializable serializable)
            : base (packetId) {

            Marker = marker;
            if (idobject != null) {
                Serial = idobject.Serial;
            } else {
                throw new ArgumentException ("Cannot update null object.");
            }
            CreatePayload (idobject.Access, serializable);
        }

        #endregion

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);
            Serial = reader.ReadUInt64 ();
            Marker = (UpdateMarker)reader.ReadInt32 ();
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);
            writer.Write (Serial);
            writer.Write ((int)Marker);
        }
    }
}

