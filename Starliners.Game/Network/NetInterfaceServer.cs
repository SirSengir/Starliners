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

﻿using System.Collections.Generic;
using BLibrary.Network;
using BLibrary.Util;
using Starliners.Game;
using System;

namespace Starliners.Network {

    /// <summary>
    /// Player specific net server which sends data and actions to the interface (client).
    /// </summary>
    public sealed class NetInterfaceServer : NetInterface {
        #region Properties

        public override Player Player {
            get;
            set;
        }

        public WorldSimulator World {
            get { return RunningState.Instance.Worlds [WorldOrdinal]; }
        }

        public IWorldAccess Access {
            get { return World.Access; }
        }

        #endregion

        public NetInterfaceServer (Networking networking)
            : base (networking) {
        }

        public void SignalSetupSent () {
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.SetupSent });
        }

        public void SignalSimulationPaused () {
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.SimulationPaused });
        }

        public void SignalSimulationUnpaused () {
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.SimulationUnpaused });
        }

        public void SendChat (TextComposition chat) {
            Networking.SendPacket (Connection, new PacketChat (PacketId.Chat, Access, chat));
        }

        public void SynchContainerData (int containerId, bool needsOpening, bool mustClose, LinkedList<DataFragment> fields) {
            Networking.SendPacket (Connection, new Packet22GuiData (containerId, Access, needsOpening, mustClose, fields));
        }

        public void CloseGui (int containerId) {
            Networking.SendPacket (Connection, new Packet22GuiData (containerId, Access, true, true, null));
        }

        public void SynchCalendar (GameClock calendar) {
            Networking.SendPacket (Connection, new Packet5Calendar { Ticks = calendar.Ticks });
        }

        public void SynchIDObject (IdObject idobject) {
            Networking.SendPacket (Connection, new Packet30Content (idobject.ContentType, idobject));
        }

        public void RemoveIdObject (IdObject idobject) {
            Networking.SendPacket (Connection, new PacketUID (PacketId.UpdateRemoved, idobject));
        }

        public void OpenGui (ushort id, int containerId) {
            Networking.SendPacket (Connection, new Packet20GuiOpen { GuiId = id, ContainerId = containerId });
        }

        public void MarkThePlayer (Player player) {
            Networking.SendPacket (Connection, new Packet4MarkPlayer (player));
        }

        public void MarkSelected (Entity entity) {
            Networking.SendPacket (Connection, new PacketUID (PacketId.EntitySelected, entity));
        }

        public void SendWorld (WorldData access) {
            Networking.SendPacket (Connection, new Packet31World (access));
        }

        public void SendViewSet (Vect2f center) {
            Networking.SendPacket (Connection, new PacketCoords (PacketId.ViewSet, center));
        }

        public void SendParticle (Particle particle) {
            Networking.SendPacket (Connection, new Packet60ParticleSpawn (particle));
        }

        public void PlaySound (string sound) {
            Networking.SendPacket (Connection, new Packet7Sound (sound));
        }

        public void PlaySound (string sound, Vect2f coordinates) {
            Networking.SendPacket (Connection, new Packet7Sound (sound, coordinates));
        }

        public void UpdateHeldObject (Player player) {
            Networking.SendPacket (Connection, new Packet24GuiPickup (player.Access, player.HeldObject));
        }

        public void UpdateState (Player player, StateObject state) {
            Networking.SendPacket (Connection, (Packet)state.GetUpdatePacket ());
        }
    }
}
