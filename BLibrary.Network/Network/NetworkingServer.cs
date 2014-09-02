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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Starliners;
using Starliners.Network;


namespace BLibrary.Network {

    sealed class NetworkingServer : Networking {
        #region Properties

        public bool AcceptsConnections {
            get;
            private set;
        }

        protected override GameConsole Console {
            get { return GameAccess.Simulator.GameConsole; }
        }

        #endregion

        #region Fields

        ManualResetEvent _connectionAccepted = new ManualResetEvent (false);
        Socket _listener;

        #endregion

        public NetworkingServer (IReadOnlyList<IPacketReader> packetReaders, IReadOnlyList<IPacketHandler> packetHandlers)
            : base (packetReaders, packetHandlers) {
        }

        public override void Stop () {
            base.Stop ();
            _connectionAccepted.Set ();
        }

        public void Listen () {

            // Establish the local endpoint for the socket.
            //IPHostEntry ipHostInfo = Dns.GetHostEntry (Dns.GetHostName ());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint (IPAddress.Any, 11000);

            // Create a TCP/IP socket.
            _listener = new Socket (AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            _listener.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try {
                _listener.Bind (localEndPoint);
                _listener.Listen (100);
                Console.Info (string.Format ("Listening on {0}:{1}...", localEndPoint.Address, localEndPoint.Port));
                AcceptsConnections = true;
                ShouldRun = true;

                while (ShouldRun) {

                    // Set the event to nonsignaled state.
                    _connectionAccepted.Reset ();

                    // Start an asynchronous socket to listen for connections.
                    Console.Debug ("Waiting for new client connection...");
                    _listener.BeginAccept (
                        new AsyncCallback (AcceptCallback),
                        _listener);

                    // Wait until a connection is made before continuing.
                    _connectionAccepted.WaitOne ();

                }

                AcceptsConnections = false;

                // We left the loop, so we need to disconnect the socket.
                if (_listener.Connected) {
                    _listener.Disconnect (false);
                }
                _listener.Close ();

            } catch (Exception ex) {
                Console.Network (ex.ToString ());
            }
        }

        void AcceptCallback (IAsyncResult ar) {
            if (!ShouldRun) {
                return;
            }

            _connectionAccepted.Set ();

            // Get the socket that handles the client request.
            Socket handler = _listener.EndAccept (ar);

            Console.Info ("New connection from " + handler.RemoteEndPoint.ToString ());

            ConnectionState connection = new ConnectionState (new NetInterfaceServer (this)) { WorkSocket = handler };
            handler.BeginReceive (connection.Buffer, 0, connection.BufferSize, 0,
                new AsyncCallback (ReadCallback), connection);
        }
    }
}
