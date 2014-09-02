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
using BLibrary.Util;
using BLibrary.Saves;
using System.Threading;
using System.Net;
using Starliners.Network;
using System.Net.NetworkInformation;

namespace Starliners {
    /// <summary>
    /// Represents the connection from the interface to the server.
    /// </summary>
    public abstract class GameConnection {

        /// <summary>
        /// Gets a value indicating whether the connection to the simulator is ready to be connected to.
        /// </summary>
        /// <value><c>true</c> if this instance is ready; otherwise, <c>false</c>.</value>
        public abstract bool IsReady {
            get;
        }

        /// <summary>
        /// Gets the remote endpoint the client networking needs to connect to.
        /// </summary>
        /// <value>The remote endpoint.</value>
        public abstract IPEndPoint RemoteEndpoint {
            get;
        }

        public abstract void Initialize ();

        public abstract void Stop ();
    }

    /// <summary>
    /// Represents the embedded server.
    /// </summary>
    public sealed class EmbeddedServer : GameConnection {

        public override bool IsReady {
            get {
                return GameAccess.Simulator.AcceptsClients;
            }
        }

        public override IPEndPoint RemoteEndpoint {
            get {
                return _endpoint;
            }
        }

        IPEndPoint _endpoint;

        #region Constructor

        public EmbeddedServer (SaveGame save)
            : this () {
            GameAccess.Simulator.SetSaveToLoad (save);
        }

        public EmbeddedServer (MetaContainer parameters, IScenarioProvider scenario)
            : this () {
            GameAccess.Simulator.SetWorldParameters (parameters, scenario);
        }

        EmbeddedServer () {

            // Log some debug info on local ip config to track issues with resolving localhost
            foreach (NetworkInterface netint in NetworkInterface.GetAllNetworkInterfaces()) {
                GameAccess.Interface.GameConsole.Network ("- NetworkInterface '{0}': {1}, {2}", netint.Name, netint.Description, netint.NetworkInterfaceType);
                foreach (UnicastIPAddressInformation info in netint.GetIPProperties().UnicastAddresses) {
                    GameAccess.Interface.GameConsole.Network ("  = Address: " + info.Address.ToString ());
                }
            }

            IPHostEntry ipHostInfo = Dns.GetHostEntry ("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList [0];
            if (ipHostInfo.AddressList.Length > 1) {
                ipAddress = ipHostInfo.AddressList [1];
            }
            _endpoint = new IPEndPoint (ipAddress, 11000);
        }

        #endregion

        public override void Initialize () {
            GameAccess.Simulator.ThreadMachina = new Thread (GameAccess.Simulator.Work) { Name = "Simulator" };
            GameAccess.Simulator.ThreadMachina.Start ();
        }

        public override void Stop () {
            if (GameAccess.Simulator != null) {
                GameAccess.Simulator.ShouldStop = true;
            }
        }

    }

    /// <summary>
    /// Represents the connection to a server not controlled by the local interface.
    /// </summary>
    public sealed class RemoteServer : GameConnection {

        public override bool IsReady {
            get {
                return true;
            }
        }

        public override IPEndPoint RemoteEndpoint {
            get {
                return _endpoint;
            }
        }

        IPEndPoint _endpoint;

        public RemoteServer (IPAddress address, int port) {
            _endpoint = new IPEndPoint (address, port);
        }

        public override void Initialize () {
            ServerCache.ServerInfo info = GameAccess.Interface.ServerCache [_endpoint.Address, _endpoint.Port];
            info.LastConnection = DateTime.Now.Ticks;
            GameAccess.Interface.ServerCache.Flush ();
        }

        public override void Stop () {
        }
    }
}

