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
using System.Collections.Generic;
using System.Runtime.Serialization;
using BLibrary.Network;
using BLibrary.Util;
using BLibrary.Gui.Data;
using Starliners.States;
using BLibrary.Audio;
using BLibrary;
using BLibrary.Gui;
using Starliners.Game;
using BLibrary.Gui.Interface;
using System.Linq;
using Starliners.Gui.Interface;

namespace Starliners.Network {

    sealed class PacketHandlerClient : IPacketHandler {
        public bool HandlePacket (INetInterface rawInterface, Packet packet) {

            NetInterfaceClient netInterface = (NetInterfaceClient)rawInterface;

            switch ((PacketId)packet.Id) {
                case PacketId.Ack:
                    OnAckPacket (netInterface, (PacketAck)packet);
                    return true;
                case PacketId.ServerInfo:
                    OnServerInfoPacket (netInterface, (Packet2ServerInfo)packet);
                    return true;
                case PacketId.PlayerMarked:
                    OnMarkPlayerPacket (netInterface, (Packet4MarkPlayer)packet);
                    return true;
                case PacketId.Sound:
                    OnSoundPacket (netInterface, (Packet7Sound)packet);
                    return true;
                case PacketId.Chat:
                    OnChatPacket (netInterface, (PacketChat)packet);
                    return true;
                case PacketId.GuiOpen:
                    OnOpenGuiPacket (netInterface, (Packet20GuiOpen)packet);
                    return true;
                case PacketId.GuiData:
                    OnContainerPacket (netInterface, (Packet22GuiData)packet);
                    return true;
                case PacketId.GuiPickup:
                    OnPickupPacket (netInterface, (Packet24GuiPickup)packet);
                    return true;
                case PacketId.Content:
                    OnContentPacket (netInterface, (Packet30Content)packet);
                    return true;
                case PacketId.World:
                    OnWorldPacket (netInterface, (Packet31World)packet);
                    return true;
                case PacketId.Calendar:
                    OnCalendarPacket (netInterface, (Packet5Calendar)packet);
                    return true;
                case PacketId.Signal:
                    OnSignalPacket (netInterface, (Packet6Signal)packet);
                    return true;
                case PacketId.UpdateRemoved:
                    OnObjectRemoved (netInterface, (PacketUID)packet);
                    return true;
                case PacketId.EntitySelected:
                    OnEntitySelectedPacket (netInterface, (PacketUID)packet);
                    return true;
                case PacketId.UpdatePayload:
                case PacketId.EntityTag:
                    OnUpdatePayloadPacket (netInterface, (PacketUpdatePayload)packet);
                    return true;
                case PacketId.UpdateStream:
                    OnUpdateStreamPacket (netInterface, (IPacketMarked)packet);
                    return true;
                case PacketId.ParticleSpawn:
                    OnParticlePacket (netInterface, (Packet60ParticleSpawn)packet);
                    return true;
                case PacketId.ViewSet:
                    OnViewSet (netInterface, (PacketCoords)packet);
                    return true;
                default:
                    return false;
            }
        }

        void OnAckPacket (NetInterfaceClient netInterface, PacketAck packet) {
            switch ((RequestIds)packet.Reference) {
                case RequestIds.Handshake:
                    netInterface.Login (0, GameAccess.Interface.Launch.Credentials);
                    break;
                case RequestIds.Login:
                    if (packet.Response == PacketAck.ResponseCode.Ok) {
                        netInterface.RequestAction (RequestIds.InfoRequest);
                    } else {
                        // TODO: Disconnect and show error message.
                    }
                    break;
                default:
                    break;
            }
        }

        void OnServerInfoPacket (NetInterfaceClient netInterface, Packet2ServerInfo packet) {
            if (!packet.IsAccepted) {
                GameAccess.Interface.GameConsole.Warning ("Connection was refused by the server.");
                return;
            }

            MapState.Instance.Controller = netInterface;
            netInterface.WorldOrdinal = 0;

            GuiManager.Instance.OpenGui (new GuiFactionSelect (packet.Worlds [0]));
        }

        void OnMarkPlayerPacket (NetInterfaceClient netInterface, Packet4MarkPlayer packet) {
            MapState.Instance.Controller = netInterface;
            netInterface.WorldOrdinal = 0;
            WorldInterface.Instance.ThePlayer = WorldInterface.Instance.Access.Players [packet.PlayerId];

            MapState.Instance.Map.CenterView (GameAccess.Interface.ThePlayer.Location);
            GameAccess.Interface.GameConsole.Debug ("Marked local player as {0}.", GameAccess.Interface.ThePlayer);
        }

        void OnSoundPacket (NetInterfaceClient netInterface, Packet7Sound packet) {
            if (packet.IsLocated) {
                if (!MapState.Instance.Map.DrawnArea.IsWithinRenderedArea (packet.Coordinates)) {
                    // Discard sound effects if they are without the rendered area.
                    return;
                }
            }
            SoundManager.Instance.Play (packet.Sound);
        }

        void OnChatPacket (NetInterfaceClient netInterface, PacketChat packet) {
            TextComposition chat = packet.DeserializeContent<TextComposition> (new StreamingContext (StreamingContextStates.Remoting, null));
            GuiChat.Instance.PushMessage (chat);
        }

        void OnEntitySelectedPacket (NetInterfaceClient netInterface, PacketUID packetUID) {
            if (packetUID.Serial != LibraryConstants.NULL_ID) {
                GameAccess.Interface.ThePlayer.SelectedEntity = WorldInterface.Instance.Access.RequireEntity (packetUID.Serial);
            } else {
                GameAccess.Interface.ThePlayer.SelectedEntity = null;
            }
        }

        void OnOpenGuiPacket (NetInterfaceClient netInterface, Packet20GuiOpen packet) {
            // If we have an existing container, we need to make sure it is maximized.
            Container container = GameAccess.Interface.ThePlayer.ContainerManager [packet.ContainerId];
            if (container != null) {
                container.BringToFront = true;
                return;
            }
            GameAccess.Interface.ThePlayer.ContainerManager.SetContainer (new Container (packet.GuiId) { ContainerId = packet.ContainerId });
        }

        void OnContainerPacket (NetInterfaceClient netInterface, Packet22GuiData packet) {

            if (packet.MustClose) {
                GuiManager.Instance.CloseGui (packet.ContainerId);
                return;
            }

            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, WorldInterface.Instance.Access);

            bool wasOpened = GameAccess.Interface.ThePlayer.ContainerManager [packet.ContainerId].NeedsOpening && !packet.NeedsOpening;
            GameAccess.Interface.ThePlayer.ContainerManager [packet.ContainerId].NeedsOpening = packet.NeedsOpening;

            LinkedList<DataFragment> fragments = packet.DecompressData (context);
            foreach (DataFragment fragment in fragments) {
                GameAccess.Interface.ThePlayer.ContainerManager [packet.ContainerId].UpdateFragment (fragment);
            }

            if (wasOpened) {
                GuiWindow gui = (GuiWindow)GameAccess.Interface.GetGuiElement (GameAccess.Interface.ThePlayer.ContainerManager [packet.ContainerId]);
                GuiManager.Instance.OpenGui (gui, packet.ContainerId);
            }
        }

        void OnPickupPacket (NetInterfaceClient netInterface, Packet24GuiPickup packet) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, WorldInterface.Instance.Access);
            if (!packet.IsNull) {
                IHoldable heldObject = packet.DecompressData (context);
                GameAccess.Interface.ThePlayer.HeldObject = heldObject;
            } else {
                GameAccess.Interface.ThePlayer.HeldObject = null;
            }

        }

        void OnContentPacket (NetInterfaceClient netInterface, Packet30Content packet) {

            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, WorldInterface.Instance.Access);

            switch (packet.Type) {
                case ContentType.Player:
                    WorldInterface.Instance.SynchPlayer (packet.DeserializeContent<Player> (context));
                    return;
                case ContentType.Asset:
                    WorldInterface.Instance.SynchAsset (packet.DeserializeContent<Asset> (context));
                    return;
                case ContentType.State:
                    WorldInterface.Instance.SynchState (packet.DeserializeContent<StateObject> (context));
                    return;
                case ContentType.Entity:
                    WorldInterface.Instance.SynchEntity (packet.DeserializeContent<Entity> (context));
                    return;
                default:
                    throw new SystemException ("Unhandled content type: " + packet.Type);
            }
        }

        void OnWorldPacket (NetInterfaceClient netInterface, Packet31World packet) {
            packet.PopulateWorld (WorldInterface.Instance);
            WorldInterface.Instance.Access.OnWorldLoaded ();
        }

        void OnCalendarPacket (NetInterfaceClient netInterface, Packet5Calendar packet) {
            GameAccess.Interface.Local.Clock.ResetCalendarTo (packet.Ticks);
        }

        void OnSignalPacket (NetInterfaceClient netInterface, Packet6Signal packet) {
            switch (packet.Type) {
                case Packet6Signal.SignalType.Heartbeat:
                    WorldInterface.Instance.Heartbeat = true;
                    netInterface.MarkHeartbeat ();
                    // Send the heartbeat back to the server.
                    netInterface.SignalHeartbeat ();
                    break;
                case Packet6Signal.SignalType.SetupSent:
                    WorldInterface.Instance.HasGameData = true;
                    WorldInterface.Instance.IsRunning = true;
                    GameAccess.Interface.GameConsole.Debug ("Server signaled completion of game data transmission.");
                    break;
                case Packet6Signal.SignalType.SimulationPaused:
                    WorldInterface.Instance.IsRunning = false;
                    GameAccess.Interface.GameConsole.Debug ("Server signaled simulation pause.");
                    break;
                case Packet6Signal.SignalType.SimulationUnpaused:
                    WorldInterface.Instance.IsRunning = true;
                    GameAccess.Interface.GameConsole.Debug ("Server signaled simulation unpause.");
                    break;
            }
        }

        void OnObjectRemoved (NetInterfaceClient netInterface, PacketUID packet) {
            IIdIdentifiable idobject = WorldInterface.Instance.Access.RequireIDObject (packet.Serial);
            Entity entity = idobject as Entity;
            if (entity != null) {
                WorldInterface.Instance.RemoveEntity (entity);
            } else {
                WorldInterface.Instance.RemoveState ((StateObject)idobject);
            }
        }

        void OnUpdatePayloadPacket (NetInterfaceClient netInterface, PacketUpdatePayload packet) {
            packet.Payload.Unpack (GameAccess.Interface.Local);
            ((IdObject)GameAccess.Interface.Local.RequireIDObject (packet.Serial)).HandleNetworkUpdate (packet);
        }

        void OnUpdateStreamPacket (NetInterfaceClient netInterface, IPacketMarked packet) {
            ((IdObject)GameAccess.Interface.Local.RequireIDObject (packet.Serial)).HandleNetworkUpdate (packet);
        }

        void OnParticlePacket (NetInterfaceClient netInterface, Packet60ParticleSpawn packet) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, GameAccess.Interface.Local);
            WorldInterface.Instance.SpawnParticle (packet.DeserializeContent<Particle> (context));
        }

        void OnViewSet (NetInterfaceClient netInterface, PacketCoords packet) {
            MapState.Instance.Map.CenterView (packet.Coordinates);
        }
    }
}
