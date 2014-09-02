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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using BLibrary.Util;
using BLibrary.Network;
using Starliners.Game;
using BLibrary.Serialization;

namespace Starliners.Network {

    public sealed class Packet2ServerInfo : Packet {

        #region Properties

        public bool IsAccepted {
            get;
            set;
        }

        public string RejectionReason {
            get;
            set;
        }

        public List<WorldInfo> Worlds {
            get;
            private set;
        }

        public override int Length {
            get {
                int length = HeaderLength + sizeof(bool) + sizeof(int);
                length += System.Text.ASCIIEncoding.Unicode.GetByteCount (RejectionReason);
                length += _payload.Length;

                return length;
            }
        }

        #endregion

        #region Fields

        byte[] _payload;

        #endregion

        #region Constructor

        public Packet2ServerInfo (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet2ServerInfo (IList<WorldSimulator> worlds)
            : this () {

            RejectionReason = "Ack";
            Worlds = new List<WorldInfo> ();
            for (int i = 0; i < worlds.Count; i++) {
                WorldSimulator world = worlds [i];
                Worlds.Add (new WorldInfo (i, world));
            }
            _payload = SerializationUtils.CompressTypeToByteArray (Worlds, new StreamingContext (StreamingContextStates.Remoting));
        }

        public Packet2ServerInfo ()
            : base ((byte)PacketId.ServerInfo) {
        }

        #endregion

        public override void ReadData (BinaryReader reader) {
            IsAccepted = reader.ReadBoolean ();
            RejectionReason = reader.ReadString ();

            int length = reader.ReadInt32 ();
            Worlds = SerializationUtils.DecompressByteArrayToType<List<WorldInfo>> (reader.ReadBytes (length), new StreamingContext (StreamingContextStates.Remoting));
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (IsAccepted);
            writer.Write (RejectionReason);

            writer.Write (_payload.Length);
            writer.Write (_payload);
        }
    }
}
