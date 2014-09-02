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
using System.Runtime.Serialization;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Serialization;

namespace Starliners.Network {

    public sealed class Packet30Content : PacketSerialized {
        public ContentType Type { get; private set; }

        public override int Length { get { return base.Length + sizeof(byte); } }

        public Packet30Content (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet30Content (ContentType type, IdObject asset)
            : base (PacketId.Content) {
            Type = type;
            CreatePayload (asset);
        }

        public Packet30Content ()
            : base (PacketId.Content) {
        }

        void CreatePayload (IdObject asset) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, asset.Access);
            Payload = SerializationUtils.CompressTypeToByteArray (asset, context);
        }

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);
            Type = (ContentType)reader.ReadByte ();
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);
            writer.Write ((byte)Type);
        }
    }
}
