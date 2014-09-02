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
using BLibrary.Serialization;
using System.Runtime.Serialization;

namespace Starliners.Network {

    sealed class Packet3Login : Packet {

        #region Properties

        public int WorldOrdinal {
            get;
            private set;
        }

        public Credentials Credentials {
            get;
            private set;
        }

        public override int Length {
            get {
                return HeaderLength
                + sizeof(int)
                + sizeof(ulong)
                + _credentials.Length;
            }
        }

        #endregion

        byte[] _credentials;

        public Packet3Login (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet3Login (int ordinal, Credentials credentials)
            : this () {
            WorldOrdinal = ordinal;
            Credentials = credentials;
            _credentials = SerializationUtils.CompressTypeToByteArray (credentials, new StreamingContext (StreamingContextStates.Remoting));
        }

        public Packet3Login ()
            : base ((byte)PacketId.Login) {
        }

        public override void ReadData (BinaryReader reader) {
            WorldOrdinal = reader.ReadInt32 ();
            int length = reader.ReadInt32 ();
            _credentials = length > 0 ? reader.ReadBytes (length) : null;
            Credentials = SerializationUtils.DecompressByteArrayToType<Credentials> (_credentials, new StreamingContext (StreamingContextStates.Remoting));
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (WorldOrdinal);
            writer.Write (_credentials.Length);
            writer.Write (_credentials);
        }

    }
}
