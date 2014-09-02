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
using System.Runtime.Serialization.Formatters.Binary;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Serialization;


namespace Starliners.Network {

    public class Packet31World : PacketSerialized {
        public Packet31World (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet31World (WorldData access)
            : base (PacketId.World) {
            CreatePayload (access);
        }

        public Packet31World ()
            : base (PacketId.World) {
        }

        void CreatePayload (WorldData access) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, access);
            Payload = SerializationUtils.CompressTypeToByteArray (access, context);
        }

        public void PopulateWorld (WorldHolder simulator) {
            IFormatter formatter = new BinaryFormatter ();
            formatter.Context = new StreamingContext (StreamingContextStates.Remoting, simulator.Access);
            formatter.Deserialize (new MemoryStream (SerializationUtils.DecompressByteArray (Payload)));
        }
    }
}
