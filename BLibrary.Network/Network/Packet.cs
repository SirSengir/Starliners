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

namespace BLibrary.Network {

    public abstract class Packet {

        byte _id;

        public byte Id {
            get {
                return _id;
            }
        }

        public abstract int Length {
            get;
        }

        public int HeaderLength {
            get {
                return sizeof(byte) + sizeof(int);
            }
        }

        public Packet (byte id, BinaryReader reader)
            : this (id) {
            ReadData (reader);
        }

        public Packet (byte id) {
            _id = id;
        }

        public abstract void ReadData (BinaryReader reader);

        public abstract void WriteData (BinaryWriter writer);

        #region Helper functions

        protected void WriteVect2f (BinaryWriter writer, Vect2f vect) {
            writer.Write (vect.X);
            writer.Write (vect.Y);
        }

        protected Vect2f ReadVect2f (BinaryReader reader) {
            return new Vect2f (reader.ReadSingle (), reader.ReadSingle ());
        }

        #endregion
    }
}
