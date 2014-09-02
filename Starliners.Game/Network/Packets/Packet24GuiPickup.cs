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

    public class Packet24GuiPickup : PacketSerialized {

        public override int Length { get { return base.Length + sizeof(bool); } }

        public bool IsNull { get; private set; }

        public Packet24GuiPickup (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet24GuiPickup (IWorldAccess access, IHoldable held)
            : base (PacketId.GuiPickup) {
            if (held == null)
                IsNull = true;
            else
                CreatePayload (access, held);
        }

        public Packet24GuiPickup ()
            : base (PacketId.GuiPickup) {
        }

        void CreatePayload (IWorldAccess access, IHoldable held) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, access);
            Payload = SerializationUtils.CompressTypeToByteArray (held, context);
        }

        public IHoldable DecompressData (StreamingContext context) {
            return SerializationUtils.DecompressByteArrayToType<IHoldable> (Payload, context);
        }

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);
            IsNull = reader.ReadBoolean ();
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);
            writer.Write (IsNull);
        }

    }

}
