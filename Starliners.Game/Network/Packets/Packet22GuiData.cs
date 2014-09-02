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

﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Serialization;


namespace Starliners.Network {

    public class Packet22GuiData : PacketSerialized {
        #region Properties

        public int ContainerId { get; set; }

        public bool NeedsOpening { get; private set; }

        public bool MustClose { get; private set; }

        public override int Length {
            get {
                return base.Length + sizeof(int) + 2 * sizeof(bool);
            }
        }

        #endregion

        #region Constructor

        public Packet22GuiData (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet22GuiData (int containerId, IWorldAccess access, bool needsOpening, bool mustClose, LinkedList<DataFragment> dataFields)
            : this () {
            ContainerId = containerId;
            NeedsOpening = needsOpening;
            MustClose = mustClose;

            if (dataFields != null) {
                StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, access);
                Payload = SerializationUtils.CompressTypeToByteArray (dataFields, context);
            }
        }

        public Packet22GuiData ()
            : base (PacketId.GuiData) {
        }

        #endregion

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);
            ContainerId = reader.ReadInt32 ();
            NeedsOpening = reader.ReadBoolean ();
            MustClose = reader.ReadBoolean ();
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);
            writer.Write (ContainerId);
            writer.Write (NeedsOpening);
            writer.Write (MustClose);
        }

        public LinkedList<DataFragment> DecompressData (StreamingContext context) {
            return SerializationUtils.DecompressByteArrayToType<LinkedList<DataFragment>> (Payload, context);
        }
    }
}
