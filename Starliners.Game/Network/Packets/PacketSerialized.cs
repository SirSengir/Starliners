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
using BLibrary.Network;
using Starliners.Game;
using System;
using BLibrary.Serialization;

namespace Starliners.Network {

    public abstract class PacketSerialized : Packet {

        protected byte[] Payload { get; set; }

        public override int Length {
            get {
                return HeaderLength + 2 * sizeof(int) + (Payload != null ? Payload.Length : 0);
            }
        }

        #region Constructor

        public PacketSerialized (PacketId id, BinaryReader reader)
            : base ((byte)id, reader) {
            ReadData (reader);
        }

        public PacketSerialized (PacketId id)
            : base ((byte)id) {
        }

        #endregion

        protected void CreatePayload (IWorldAccess access, object serializable) {
            if (serializable == null) {
                return;
            }
            Payload = SerializationUtils.CompressTypeToByteArray (serializable, new StreamingContext (StreamingContextStates.Remoting, access));
        }

        public T DeserializeContent<T> (StreamingContext context) where T : class {
            if (Payload == null) {
                return null;
            }
            return SerializationUtils.DecompressByteArrayToType<T> (Payload, context);
        }

        public T DeserializeContent<T> (IWorldAccess access) where T : class {
            if (Payload == null) {
                return null;
            }

            return SerializationUtils.DecompressByteArrayToType<T> (Payload, new StreamingContext (StreamingContextStates.Remoting, access));
        }

        public override void ReadData (BinaryReader reader) {
            int length = reader.ReadInt32 ();
            Payload = length > 0 ? reader.ReadBytes (length) : null;
        }

        public override void WriteData (BinaryWriter writer) {
            if (Payload != null) {
                writer.Write (Payload.Length);
                writer.Write (Payload);
            } else {
                writer.Write (0);
            }
        }
    }
}
