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

    public sealed class Packet60ParticleSpawn : PacketSerialized {

        public Packet60ParticleSpawn (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet60ParticleSpawn (Particle particle)
            : this () {
            CreatePayload (particle);
        }

        public Packet60ParticleSpawn ()
            : base (PacketId.ParticleSpawn) {
        }

        void CreatePayload (Particle particle) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, particle.Access);
            Payload = SerializationUtils.CompressTypeToByteArray (particle, context);
        }


    }
}
