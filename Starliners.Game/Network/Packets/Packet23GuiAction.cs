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
using System.IO;
using BLibrary.Network;
using Starliners.Game;

namespace Starliners.Network {

    sealed class Packet23GuiAction : Packet {

        public int ContainerId { get; set; }

        public String Key { get; set; }

        public Payload Payload { get; set; }

        public override int Length {
            get {
                return HeaderLength + sizeof(int) + System.Text.ASCIIEncoding.Unicode.GetByteCount (Key) + Payload.Length;
            }
        }

        public Packet23GuiAction (BinaryReader reader)
            : this () {
            ReadData (reader);
        }

        public Packet23GuiAction (IWorldAccess access, int containerId, string key, params object[] args)
            : this () {
            ContainerId = containerId;
            Key = key;
            Payload = new Payload (access, args);
        }

        public Packet23GuiAction ()
            : base ((byte)PacketId.GuiAction) {
        }

        #region implemented abstract members of Packet

        public override void ReadData (BinaryReader reader) {
            ContainerId = reader.ReadInt32 ();
            Key = reader.ReadString ();
            Payload = new Payload (reader);
        }

        public override void WriteData (BinaryWriter writer) {
            writer.Write (ContainerId);
            writer.Write (Key);
            Payload.WriteData (writer);
        }

        #endregion
    }
}
