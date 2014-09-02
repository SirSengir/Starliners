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

﻿using BLibrary.Network;
using BLibrary.Util;
using Starliners.Game;

namespace Starliners.Network {

    public class NetInterfaceClient : NetInterface {
        public override Player Player {
            get { return GameAccess.Interface.ThePlayer; }
            set { }
        }

        public NetInterfaceClient (Networking networking)
            : base (networking) {
        }

        public void Login (int ordinal, Credentials credentials) {
            Networking.SendPacket (Connection, new Packet3Login (ordinal, credentials));
        }

        public void SelectFaction (ulong serial) {
            Networking.SendPacket (Connection, new PacketUID (PacketId.FactionSelect, serial));
        }

        public void ClearHeld () {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.HeldCleared });
        }

        public void RequestSave () {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.RequestSave });
        }

        public void SetPlayerLocation (Vect2f coords) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new PacketCoords (PacketId.ViewSet, coords));
        }

        /// <summary>
        /// Informs the simulator about player movement on the interface.
        /// </summary>
        /// <remarks>If the simulator determines that the given new location is not valid for the given delta, it should attempt to correct the player position on the interface.</remarks>
        /// <param name="delta">Delta.</param>
        /// <param name="relocated">New location reached after applying delta as determined by the interface.</param>
        public void MovedPlayer (Vect2f delta, Vect2f relocated) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new PacketUpdatePayload (PacketId.UpdatePayload, Player, UpdateMarker.Update2, delta.X, delta.Y, relocated.X, relocated.Y));
        }

        #region Map Interaction

        public void ActivatedEntity (Entity entity, ControlState control) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new PacketUIDClick (PacketId.EntityTargeted, entity, control));
        }

        public void ClickedEntity (Entity entity, ControlState control) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new PacketUIDClick (PacketId.EntityClick, entity, control));
        }

        public void PulseEntity (Entity entity) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new PacketUID (PacketId.EntityPulse, entity));
        }

        public void ActivatedMap (Vect2f coordinates, ControlState control) {
            if (!IsBound)
                return;
            Networking.SendPacket (Connection, new PacketCoordsClick (PacketId.MapTargeted, coordinates, control));
        }

        public void ClickedMap (Vect2f coordinates, ControlState control) {
            if (!IsBound)
                return;
            Networking.SendPacket (Connection, new PacketCoordsClick (PacketId.MapClick, coordinates, control));
        }

        #endregion

        public void ClosedGui (int containerId) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new Packet21GuiClosed { ContainerId = containerId });
        }

        public void RequestAction (RequestIds request) {
            Networking.SendPacket (Connection, new PacketRequest (request));
        }

        public void ActionGui (int containerId, string key, params object[] args) {
            if (!IsBound) {
                return;
            }
            Networking.SendPacket (Connection, new Packet23GuiAction (Player.Access, containerId, key, args));
        }
    }
}
