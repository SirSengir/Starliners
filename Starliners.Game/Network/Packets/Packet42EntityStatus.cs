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
using Starliners.Game;


namespace Starliners.Network {

    public sealed class Packet42EntityStatus : PacketUID, IPacketMarked {
        public UpdateMarker Marker { get { return UpdateMarker.Status; } }

        public EntityStatus.StatusLevel Level { get; private set; }

        public EntityStatus.StatusSymbol Symbol { get; private set; }

        public string Message { get; private set; }

        public override int Length {
            get { return base.Length + sizeof(byte) + System.Text.ASCIIEncoding.Unicode.GetByteCount (Message); }
        }

        public Packet42EntityStatus (BinaryReader reader)
            : base (PacketId.EntityStatus) {
            ReadData (reader);
        }

        public Packet42EntityStatus (Entity entity)
            : base (PacketId.EntityStatus, entity) {
            if (entity.Status != null) {
                Level = entity.Status.Level;
                Symbol = entity.Status.Symbol;
                Message = entity.Status.Message;
            } else {
                Level = EntityStatus.StatusLevel.None;
                Symbol = EntityStatus.StatusSymbol.None;
                Message = string.Empty;
            }
        }

        public Packet42EntityStatus ()
            : base (PacketId.EntityStatus) {
        }

        #region implemented abstract members of Packet

        public override void ReadData (BinaryReader reader) {
            base.ReadData (reader);

            Level = (EntityStatus.StatusLevel)reader.ReadByte ();
            if (Level != EntityStatus.StatusLevel.None) {
                Symbol = (EntityStatus.StatusSymbol)reader.ReadByte ();
                Message = reader.ReadString ();
            }
        }

        public override void WriteData (BinaryWriter writer) {
            base.WriteData (writer);

            writer.Write ((byte)Level);
            if (Level != EntityStatus.StatusLevel.None) {
                writer.Write ((byte)Symbol);
                writer.Write (Message);
            }
        }

        #endregion
    }
}
