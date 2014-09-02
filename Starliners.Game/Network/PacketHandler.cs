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
using BLibrary.Gui.Data;
using Starliners.Game;
using BLibrary.Util;

namespace Starliners.Network {

    sealed class PacketHandler : IPacketHandler {

        public bool HandlePacket (INetInterface rawInterface, Packet packet) {

            NetInterfaceServer netInterface = (NetInterfaceServer)rawInterface;

            switch ((PacketId)packet.Id) {
                case PacketId.Login:
                    OnLoginPacket (netInterface, (Packet3Login)packet);
                    return true;
                case PacketId.FactionSelect:
                    OnFactionSelectPacket (netInterface, (PacketUID)packet);
                    return true;
                case PacketId.Signal:
                    OnSignalPacket (netInterface, (Packet6Signal)packet);
                    return true;
                case PacketId.ViewSet:
                    OnViewSet (netInterface, (PacketCoords)packet);
                    return true;
                case PacketId.GuiAction:
                    OnGuiAction (netInterface, (Packet23GuiAction)packet);
                    return true;
                case PacketId.GuiClosed:
                    OnClosedGuiPacket (netInterface, (Packet21GuiClosed)packet);
                    return true;
                case PacketId.Request:
                    OnRequestPacket (netInterface, (PacketRequest)packet);
                    return true;
                case PacketId.EntityTargeted:
                    OnEntityTargetedPacket (netInterface, (PacketUIDClick)packet);
                    return true;
                case PacketId.EntityClick:
                    OnEntityClickPacket (netInterface, (PacketUIDClick)packet);
                    return true;
                case PacketId.EntityPulse:
                    OnEntityPulsePacket (netInterface, (PacketUID)packet);
                    return true;
                case PacketId.UpdatePayload:
                    OnUpdatePayloadPacket (netInterface, (PacketUpdatePayload)packet);
                    return true;
                case PacketId.MapTargeted:
                    PacketCoordsClick pack0 = (PacketCoordsClick)packet;
                    netInterface.World.MapTargeted (netInterface.Player, pack0.Coordinates, pack0.Control);
                    return true;
                case PacketId.MapClick:
                    PacketCoordsClick pack1 = (PacketCoordsClick)packet;
                    netInterface.World.MapClicked (netInterface.Player, pack1.Coordinates, pack1.Control);
                    return true;
                default:
                    return false;
            }
        }

        void OnRequestPacket (NetInterfaceServer netInterface, PacketRequest packet) {
            switch (packet.Type) {
                case RequestIds.Handshake:
                    netInterface.SendPacket (new PacketAck (PacketId.Ack, PacketAck.ResponseCode.Ok, (byte)packet.Type) { Note = PlatformUtils.GetEXEVersion ().ToString () });
                    return;
                case RequestIds.InfoRequest:
                    netInterface.SendPacket (new Packet2ServerInfo (RunningState.Instance.Worlds) { IsAccepted = true });
                    return;
                default:
                    GameAccess.Game.HandleRequest (netInterface, (byte)packet.Type);
                    return;
            }
        }

        void OnLoginPacket (NetInterfaceServer netInterface, Packet3Login packet) {
            netInterface.WorldOrdinal = packet.WorldOrdinal;

            Player player = null;
            foreach (Player candidate in netInterface.World.Access.Players.Values) {
                if (candidate.Name.Equals (packet.Credentials.Login)) {
                    player = candidate;
                }
            }
            if (player == null) {
                player = netInterface.World.Populator.CreatePlayer (netInterface.World.Access, packet.Credentials.Login);
                netInterface.World.AddPlayer (player);
            } else {
                // Don't login twice.
                if (netInterface.World.IsOnline (player)) {
                    netInterface.SendPacket (new PacketAck (PacketId.Ack, PacketAck.ResponseCode.NotOk, (byte)RequestIds.Login) { Note = string.Format ("Player '{0}' is already online.", player.Name) });
                } else {
                    player.ContainerManager.Clear ();
                }
            }

            netInterface.Player = player;
            if (player.MainFaction != null) {
                netInterface.World.JoinPlayer (player, netInterface);
            } else {
                netInterface.SendPacket (new PacketAck (PacketId.Ack, PacketAck.ResponseCode.Ok, (byte)RequestIds.Login) { Note = "Login OK" });
            }
        }

        void OnFactionSelectPacket (NetInterfaceServer netInterface, PacketUID packet) {
            Faction faction = netInterface.Access.GetState (packet.Serial) as Faction;
            if (faction == null) {
                return;
            }
            if (netInterface.Player == null) {
                return;
            }
            netInterface.Player.MakeController (faction);
            netInterface.World.JoinPlayer (netInterface.Player, netInterface);
        }

        void OnSignalPacket (NetInterfaceServer netInterface, Packet6Signal packet) {
            switch (packet.Type) {
                case Packet6Signal.SignalType.Heartbeat:
                    netInterface.MarkHeartbeat ();
                    break;
                case Packet6Signal.SignalType.HeldCleared:
                    netInterface.World.UpdateHeld (netInterface.Player, null);
                    break;
                case Packet6Signal.SignalType.RequestSave:
                    netInterface.World.Save (true);
                    break;
            }
        }

        void OnViewSet (NetInterfaceServer netInterface, PacketCoords packet) {
            netInterface.Player.Location = packet.Coordinates;
        }

        void OnGuiAction (NetInterfaceServer netInterface, Packet23GuiAction packet) {
            Container container = netInterface.Player.ContainerManager [packet.ContainerId];
            if (container == null) {
                return;
            }

            // Unpack fragments
            packet.Payload.Unpack (netInterface.Access);
            container.HandleAction (netInterface.Player, packet.Key, packet.Payload);
        }

        void OnClosedGuiPacket (NetInterfaceServer netInterface, Packet21GuiClosed packet) {
            netInterface.World.ClosedGui (netInterface.Player.Serial, packet.ContainerId);
        }

        void OnEntityClickPacket (NetInterfaceServer netInterface, PacketUIDClick packet) {
            netInterface.World.EntityClick (packet.Serial, netInterface.Player, packet.Control);
        }

        void OnEntityTargetedPacket (NetInterfaceServer netInterface, PacketUIDClick packet) {
            netInterface.World.EntityTargeted (packet.Serial, netInterface.Player, packet.Control);
        }

        void OnEntityPulsePacket (NetInterfaceServer netInterface, PacketUID packet) {
            Entity entity = netInterface.World.Access.GetEntity (packet.Serial);
            // It is possible that the entity still exists on the interface, resulting
            // in pulses, even if the entity is already removed on the simulator.
            if (entity != null) {
                entity.OnUpdatePulse (netInterface.Player);
            }
        }

        void OnUpdatePayloadPacket (NetInterfaceServer netInterface, PacketUpdatePayload packet) {
            packet.Payload.Unpack (netInterface.World.Access);
            ((IdObject)netInterface.World.Access.RequireIDObject (packet.Serial)).HandleNetworkUpdate (packet);
        }
    }
}
