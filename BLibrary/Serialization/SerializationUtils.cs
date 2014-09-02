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
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace BLibrary.Serialization {

    public static class SerializationUtils {

        public static byte[] CompressTypeToByteArray<T> (T input, StreamingContext context) where T : class {
            byte[] uncompressed = SerializeTypeToByteArray (input, context);
            return CompressByteArray (uncompressed);
        }

        public static T DecompressByteArrayToType<T> (byte[] buffer, StreamingContext context) where T : class {
            byte[] decompressed = DecompressByteArray (buffer);
            return DeserializeByteArrayToType<T> (decompressed, context);
        }

        public static byte[] DecompressByteArray (byte[] compressedByteArray) {

            MemoryStream compressedStream = new MemoryStream (compressedByteArray);
            DeflateStream compressedzipStream =
                new DeflateStream (compressedStream, CompressionMode.Decompress, true);
            MemoryStream decompressedStream = new MemoryStream ();
            const int blockSize = 1024;

            byte[] buffer = new byte[blockSize];
            int bytesRead;
            while ((bytesRead = compressedzipStream.Read (buffer, 0, buffer.Length)) != 0) {
                decompressedStream.Write (buffer, 0, bytesRead);
            }

            compressedzipStream.Close ();
            decompressedStream.Position = 0;
            byte[] decompressedArray = decompressedStream.ToArray ();
            decompressedStream.Close ();
            decompressedStream.Dispose ();
            compressedzipStream.Close ();
            compressedzipStream.Dispose ();
            return decompressedArray;
        }

        static byte[] CompressByteArray (byte[] uncompressedByteArray) {

            MemoryStream msCompressed = new MemoryStream ();
            DeflateStream compressedzipStream =
                new DeflateStream (msCompressed, CompressionMode.Compress, true);

            compressedzipStream.Write (uncompressedByteArray, 0, uncompressedByteArray.Length);
            compressedzipStream.Flush ();
            compressedzipStream.Close ();

            byte[] compressedByteArray = msCompressed.ToArray ();
            msCompressed.Close ();
            msCompressed.Dispose ();

            return compressedByteArray;
        }

        static T DeserializeStreamToType<T> (Stream stream) where T : class {
            BinaryFormatter formatter = new BinaryFormatter ();
            T deserialized = formatter.Deserialize (stream) as T;
            return deserialized;
        }

        static T DeserializeByteArrayToType<T> (byte[] bytes, StreamingContext context) where T : class {

            BinaryFormatter formatter = new BinaryFormatter (null, context);
            MemoryStream stream = new MemoryStream (bytes);
            T deserialized = formatter.Deserialize (stream) as T;
            stream.Close ();
            stream.Dispose ();
            return deserialized;
        }

        static byte[] SerializeTypeToByteArray<T> (T input, StreamingContext context) where T : class {
            BinaryFormatter formatter = new BinaryFormatter (null, context);
            MemoryStream stream = new MemoryStream ();
            formatter.Serialize (stream, input);

            byte[] array = stream.ToArray ();
            stream.Close ();
            stream.Dispose ();
            return array;
        }

    }
}

