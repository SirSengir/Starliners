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
using BLibrary.Util;

namespace Starliners.Network {

    public sealed class PacketCoordsClick : PacketCoords {
        public ControlState Control {
            get;
            private set;
        }

        public override int Length {
            get { return base.Length + sizeof(int); }
        }

        public PacketCoordsClick (PacketId packetid, BinaryReader reader)
            : this (packetid) {
            ReadData (reader);
        }

        public PacketCoordsClick (PacketId packetid, Vect2f coords, ControlState control)
            : base (packetid, coords) {
            Control = control;
        }

        public PacketCoordsClick (PacketId packetid)
            : base (packetid) {
        }

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);
            Control = (ControlState)reader.ReadInt32 ();
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);
            writer.Write ((int)Control);
        }
    }
}

