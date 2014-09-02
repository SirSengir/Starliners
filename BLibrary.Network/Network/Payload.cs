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

﻿using System.IO;
using System.Runtime.Serialization;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Serialization;

namespace BLibrary.Network {

    /// <summary>
    /// Payload for gui actions and entity updates.
    /// </summary>
    public sealed class Payload {

        byte[] _serialized;

        public int Length {
            get {
                int length = sizeof(int);
                if (_serialized != null)
                    length += _serialized.Length;

                return length;
            }
        }

        #region Constructor

        public Payload (BinaryReader reader) {
            ReadData (reader);
        }

        public Payload (IWorldAccess access, params object[] args) {
            StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, access);
            _serialized = SerializationUtils.CompressTypeToByteArray (args, context);
        }

        #endregion

        #region Fragment handling

        object[] _dataFragments;

        /// <summary>
        /// Prepares carried data fragments by unpacking them from the byte stream.
        /// </summary>
        /// <param name="access">Access.</param>
        public void Unpack (IWorldAccess access) {
            if (_serialized == null)
                return;

            _dataFragments = SerializationUtils.DecompressByteArrayToType<object[]> (_serialized, new StreamingContext (StreamingContextStates.Remoting, access));
        }

        public bool HasFragment (int index) {
            return index < _dataFragments.Length;
        }

        public T GetValue<T> (int index) {
            return (T)_dataFragments [index];
        }

        #endregion

        public void ReadData (BinaryReader reader) {
            int count = reader.ReadInt32 ();
            if (count > 0) {
                _serialized = reader.ReadBytes (count);
            }
        }

        public void WriteData (BinaryWriter writer) {
            if (_serialized == null)
                writer.Write (0);
            else {
                writer.Write (_serialized.Length);
                writer.Write (_serialized);
            }
        }

    }
}
