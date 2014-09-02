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
using Starliners.Game;
using System;

namespace Starliners.Network {

    public abstract class NetInterface : INetInterface {

        #region Properties

        public Networking Networking {
            get;
            protected set;
        }

        public ConnectionState Connection {
            get;
            set;
        }

        public abstract Player Player {
            get;
            set;
        }

        public int WorldOrdinal {
            get;
            set;
        }

        protected bool IsBound {
            get { return WorldOrdinal >= 0; }
        }

        public bool HasDisconnect {
            get {
                return DateTime.Now.Ticks - _lastHeartbeat > TimeSpan.TicksPerSecond * 20;
            }
        }

        #endregion

        long _lastHeartbeat;

        public NetInterface (Networking networking) {
            Networking = networking;
            WorldOrdinal = -1;
            MarkHeartbeat ();
        }

        public void MarkHeartbeat () {
            _lastHeartbeat = DateTime.Now.Ticks;
        }

        public void SendPacket (Packet packet) {
            Networking.SendPacket (Connection, packet);
        }

        public void SignalHeartbeat () {
            Networking.SendPacket (Connection, new Packet6Signal () { Type = Packet6Signal.SignalType.Heartbeat });
        }

    }
}
