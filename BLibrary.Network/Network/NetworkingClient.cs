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
using BLibrary.Util;

namespace BLibrary.Network {

    public sealed class NetworkingClient : Networking {

        #region Properties

        public override long BytesReceived {
            get { return _connection != null ? _connection.PartialBuffered : 0; }
        }

        public override long BytesExpected {
            get { return _connection != null ? _connection.ExpectedPacketSize : 0; }
        }

        public bool IsConnected {
            get;
            private set;
        }

        protected override GameConsole Console {
            get { return GameAccess.Interface.GameConsole; }
        }

        #endregion

        #region Events

        public NetworkEventHandler ConnectionClosed;

        #endregion

        #region Fields

        ConnectionState _connection;
        ManualResetEvent _connectDone = new ManualResetEvent (false);
        ManualResetEvent _receiveDone = new ManualResetEvent (false);

        IPEndPoint _endpoint;

        #endregion

        public NetworkingClient (IReadOnlyList<IPacketReader> packetReaders, IReadOnlyList<IPacketHandler> packetHandlers, IPEndPoint endpoint)
            : base (packetReaders, packetHandlers) {
            _endpoint = endpoint;
        }

        public override void Stop () {
            base.Stop ();
            _receiveDone.Set ();
        }

        public void Connect () {
            try {
                // Create a TCP/IP socket.
                Socket client = new Socket (_endpoint.Address.AddressFamily,
                                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect (_endpoint,
                    new AsyncCallback (ConnectCallback), client);
                _connectDone.WaitOne ();

                SendPacket (_connection, new PacketRequest (RequestIds.Handshake));

                // Receive packets
                while (ShouldRun && _connection.WorkSocket.Connected) {
                    // Wait for the next packet if we are active.
                    client.BeginReceive (_connection.Buffer, 0, _connection.BufferSize, 0, new AsyncCallback (ReadCallback), _connection);
                    _receiveDone.WaitOne ();
                }

                IsConnected = false;
                // Close socket if we are supposed to stop.
                try {
                    if (_connection != null && _connection.WorkSocket.Connected) {
                        Console.Network ("Disconnecting socket.");
                        _connection.WorkSocket.Disconnect (false);
                        _connection.WorkSocket.Shutdown (SocketShutdown.Both);
                    }
                } catch (Exception ex) {
                    Console.Network (ex.ToString ());
                }

                Console.Network ("Client networking stopped.");

            } catch (Exception ex) {
                Console.Network (ex.ToString ());
            }
        }

        void ConnectCallback (IAsyncResult ar) {

            try {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect (ar);
                _connection = new ConnectionState (new NetInterfaceClient (this)) { WorkSocket = client };

                IsConnected = true;
                Console.Network (string.Format ("Socket connected to {0}", client.RemoteEndPoint.ToString ()));

            } catch (Exception e) {
                if (ConnectionClosed != null) {
                    ConnectionClosed (this, new CustomEventArgs<string> (e.Message));
                }
                Console.Network (e.ToString ());
                Stop ();
            }

            // Signal that the connection has been made.
            _connectDone.Set ();
        }
    }
}
