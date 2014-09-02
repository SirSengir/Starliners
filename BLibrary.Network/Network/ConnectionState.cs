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
using System.Net.Sockets;

namespace BLibrary.Network {
    public sealed class ConnectionState {

        #region Properties

        internal INetInterface NetInterface {
            get;
            private set;
        }

        public int BufferSize {
            get { return Buffer.Length; }
        }

        internal Socket WorkSocket {
            get;
            set;
        }

        public bool IsBuffering {
            get;
            private set;
        }

        public int PartialBuffered {
            get;
            private set;
        }

        public int ExpectedPacketSize {
            get;
            private set;
        }

        public byte[] Buffer = new byte[Networking.BUFFER_SIZE];

        public byte[] Unprocessed {
            get;
            set;
        }

        #endregion

        BinaryWriter _partialWriter;

        internal ConnectionState (INetInterface netInterface) {
            NetInterface = netInterface;
            netInterface.Connection = this;
        }

        public void StartPartialPacket (int packetSize) {
            IsBuffering = true;
            PartialBuffered = 0;
            ExpectedPacketSize = packetSize;
            _partialWriter = new BinaryWriter (new MemoryStream (new byte[packetSize]));
        }

        public void AppendPartial (byte[] bytes, int index, int count) {
            _partialWriter.Write (bytes, index, count);
            PartialBuffered += count;
        }

        public byte[] EndPartialPacket (byte[] bytes) {

            IsBuffering = false;
            // Finish writing to stream
            _partialWriter.Write (bytes, 0, ExpectedPacketSize - PartialBuffered);
            PartialBuffered = ExpectedPacketSize;
            _partialWriter.Flush ();
            _partialWriter.BaseStream.Position = 0;

            BinaryReader reader = new BinaryReader (_partialWriter.BaseStream);
            _partialWriter = null;
            return reader.ReadBytes ((int)reader.BaseStream.Length);
        }
    }
}
