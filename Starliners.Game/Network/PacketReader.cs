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
using BLibrary.Network;
using System.IO;

namespace Starliners.Network {
    public sealed class PacketReader : IPacketReader {
        public Packet ReadPacket (byte packetId, BinaryReader reader) {
            switch ((PacketId)packetId) {
                case PacketId.Request:
                    return new PacketRequest (reader);
                case PacketId.Ack:
                    return new PacketAck ((PacketId)packetId, reader);
                case PacketId.ServerInfo:
                    return new Packet2ServerInfo (reader);
                case PacketId.Login:
                    return new Packet3Login (reader);
                case PacketId.PlayerMarked:
                    return new Packet4MarkPlayer (reader);
                case PacketId.Calendar:
                    return new Packet5Calendar (reader);
                case PacketId.Signal:
                    return new Packet6Signal (reader);
                case PacketId.Sound:
                    return new Packet7Sound (reader);
                case PacketId.ViewSet:
                    return new PacketCoords ((PacketId)packetId, reader);
                case PacketId.Chat:
                    return new PacketChat ((PacketId)packetId, reader);
                case PacketId.GuiOpen:
                    return new Packet20GuiOpen (reader);
                case PacketId.GuiClosed:
                    return new Packet21GuiClosed (reader);
                case PacketId.GuiData:
                    return new Packet22GuiData (reader);
                case PacketId.GuiAction:
                    return new Packet23GuiAction (reader);
                case PacketId.GuiPickup:
                    return new Packet24GuiPickup (reader);
                case PacketId.Content:
                    return new Packet30Content (reader);
                case PacketId.World:
                    return new Packet31World (reader);
                case PacketId.EntityStatus:
                    return new Packet42EntityStatus (reader);

                case PacketId.MapClick:
                case PacketId.MapTargeted:
                    return new PacketCoordsClick ((PacketId)packetId, reader);

                case PacketId.EntityClick:
                case PacketId.EntityTargeted:
                    return new PacketUIDClick ((PacketId)packetId, reader);

                case PacketId.FactionSelect:
                case PacketId.UpdateRemoved:
                case PacketId.EntitySelected:
                case PacketId.EntityPulse:
                    return new PacketUID ((PacketId)packetId, reader);

                case PacketId.EntityTag:
                case PacketId.UpdatePayload:
                    return new PacketUpdatePayload ((PacketId)packetId, reader);

                case PacketId.UpdateStream:
                    return new PacketUpdateStream ((PacketId)packetId, reader);

                case PacketId.ParticleSpawn:
                    return new Packet60ParticleSpawn (reader);

                default:
                    return null;
            }
        }
    }
}

