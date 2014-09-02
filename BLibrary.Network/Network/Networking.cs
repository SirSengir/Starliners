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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using BLibrary.Util;

namespace BLibrary.Network {

    public delegate void NetworkEventHandler (Networking sender, CustomEventArgs<string> args);

    public abstract class Networking {
        #region Constants

        public const int BUFFER_SIZE = 65536;

        #endregion

        #region Properties

        protected bool ShouldRun {
            get { return _shouldRun; }
            set { _shouldRun = value; }
        }

        protected abstract GameConsole Console {
            get;
        }

        public bool HasPendingPackets {
            get { return _queued.Count > 0; }
        }

        public virtual long BytesReceived { get { return 0; } }

        public virtual long BytesExpected { get { return 0; } }

        #endregion

        #region Fields

        IReadOnlyList<IPacketReader> _packetReaders;
        IReadOnlyList<IPacketHandler> _packetHandlers;

        ConcurrentQueue<PacketDispatch> _queued = new ConcurrentQueue<PacketDispatch> ();
        bool _shouldRun = true;

        #endregion

        public Networking (IReadOnlyList<IPacketReader> packetReaders, IReadOnlyList<IPacketHandler> packetHandlers) {
            _packetReaders = packetReaders;
            _packetHandlers = packetHandlers;
        }

        public void DispatchPackets () {

            PacketDispatch dispatch;
            bool didDequeue = false;
            while (HasPendingPackets) {
                didDequeue = _queued.TryDequeue (out dispatch);
                if (didDequeue) {
                    foreach (IPacketHandler handler in _packetHandlers) {
                        if (handler.HandlePacket (dispatch.NetInterface, dispatch.Packet)) {
                            break;
                        }
                    }
                }
            }
        }

        public virtual void Stop () {
            ShouldRun = false;
        }

        protected void ReadCallback (IAsyncResult ar) {
            try {
                ConnectionState connection = (ConnectionState)ar.AsyncState;

                int bytesRead = connection.WorkSocket.EndReceive (ar);
                if (bytesRead <= 0)
                    return;

                byte[] bytes = connection.Buffer;
                int readBytes = bytesRead;
                if (connection.Unprocessed != null && connection.Unprocessed.Length > 0) {
                    //Console.LowLevel("Prepending unprocessed " + connection.Unprocessed.Length + " bytes from previous iteration.");
                    byte[] prepend = new byte[bytes.Length + connection.Unprocessed.Length];
                    Buffer.BlockCopy (connection.Unprocessed, 0, prepend, 0, connection.Unprocessed.Length);
                    Buffer.BlockCopy (bytes, 0, prepend, connection.Unprocessed.Length, bytes.Length);
                    readBytes = connection.Unprocessed.Length + readBytes;
                    bytes = prepend;
                    connection.Unprocessed = null;
                }

                if (connection.IsBuffering) {
                    connection.Unprocessed = ReadPartial (connection, readBytes, bytes);
                } else {
                    connection.Unprocessed = ReadStart (connection, readBytes, bytes);
                }

                // Restart receive
                if (connection.WorkSocket.Connected) {
                    connection.WorkSocket.BeginReceive (connection.Buffer, 0, connection.BufferSize, 0,
                        new AsyncCallback (ReadCallback), connection);
                }

            } catch (Exception ex) {
                Console.Network (ex.StackTrace);
            }

        }

        byte[] ReadStart (ConnectionState connection, int bytesRead, byte[] bytes) {

            // Abort if we don't have enough for the next header.
            if (bytesRead <= sizeof(byte) + sizeof(int)) {
                return bytes;
            }

            BinaryReader reader = new BinaryReader (new MemoryStream (bytes));
            /*byte packetId = */
            reader.ReadByte ();
            int packetSize = reader.ReadInt32 ();

            if (bytesRead < packetSize) {

                connection.StartPartialPacket (packetSize);
                connection.AppendPartial (bytes, 0, bytesRead);

            } else if (packetSize < bytesRead) {

                HandlePacketStream (connection, bytes);
                byte[] remain = new byte[bytesRead - packetSize];
                //Console.Debug(String.Format("Packet with size {0} done, some bytes remaining: {1}", packetSize, bytesRead));
                Buffer.BlockCopy (bytes, packetSize, remain, 0, remain.Length);
                return ReadStart (connection, remain.Length, remain);
            } else {
                HandlePacketStream (connection, bytes);
            }

            return null;
        }

        byte[] ReadPartial (ConnectionState connection, int bytesRead, byte[] bytes) {
            int bufferingStillRequired = connection.ExpectedPacketSize - connection.PartialBuffered;

            if (bufferingStillRequired <= 0) {
                throw new SystemException ("Failed to correctly stop partial packet processing.");
            }
            if (bufferingStillRequired > bytesRead) {

                connection.AppendPartial (bytes, 0, bytesRead);

            } else if (bufferingStillRequired < bytesRead) {

                HandlePacketStream (connection, connection.EndPartialPacket (bytes));
                byte[] remain = new byte[bytesRead - bufferingStillRequired];
                Buffer.BlockCopy (bytes, bufferingStillRequired, remain, 0, remain.Length);
                return ReadStart (connection, remain.Length, remain);

            } else {
                HandlePacketStream (connection, connection.EndPartialPacket (bytes));
            }

            return null;
        }

        protected void HandlePacketStream (ConnectionState connection, byte[] bytes) {

            BinaryReader reader = new BinaryReader (new MemoryStream (bytes));

            byte packetId = reader.ReadByte ();
            int packetSize = reader.ReadInt32 ();

            // Discard invalid packets
            if (packetSize > reader.BaseStream.Length) {
                Console.Network ("Discarding packet (" + packetId.ToString () + ") from " + connection.WorkSocket.RemoteEndPoint.ToString () + " because actual bytes did not match expected length: " + reader.BaseStream.Length + " <-> " + packetSize);
                return;
            }

            QueuePacket (connection, packetId, reader);
        }

        protected void QueuePacket (ConnectionState connection, byte packetId, BinaryReader reader) {

            Packet packet = null;
            foreach (IPacketReader preader in _packetReaders) {
                packet = preader.ReadPacket (packetId, reader);
                if (packet != null) {
                    break;
                }
            }
            if (packet == null) {
                Console.Network ("Discarded unknown packet id: " + packetId.ToString ());
                return;
            }

            _queued.Enqueue (new PacketDispatch (connection.NetInterface, packet));
        }

        public void SendPacket (ConnectionState connection, Packet packet) {
            if (packet == null) {
                return;
            }

            //Console.LowLevel("Sending packet (" + ((PacketId)packet.Id).ToString() + ") to " + connection.WorkSocket.RemoteEndPoint.ToString() + " with a size of " + packet.Length + " bytes.");
            BinaryWriter writer = new BinaryWriter (new MemoryStream (new byte[packet.Length]));
            writer.Write (packet.Id);
            writer.Write (packet.Length);
            packet.WriteData (writer);

            writer.Flush ();
            writer.BaseStream.Position = 0;
            byte[] bytes = new BinaryReader (writer.BaseStream).ReadBytes ((int)writer.BaseStream.Length);

            if (bytes.Length != packet.Length) {
                Console.Network ("Discarded packet (" + packet.Id.ToString () + ") to " + connection.WorkSocket.RemoteEndPoint.ToString () + " because actual bytes did not match expected length: " + bytes.Length + " <-> " + packet.Length);
                return;
            }

            try {
                if (connection.WorkSocket.Connected)
                    connection.WorkSocket.BeginSend (bytes, 0, bytes.Length, 0, new AsyncCallback (SendCallback), connection);
            } catch (Exception ex) {
                Console.Network ("Caught and hid a socket exception: " + ex.Message);
            }
        }

        protected void SendCallback (IAsyncResult ar) {
        }
    }
}
