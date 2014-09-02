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
using System.IO;
using System.IO.Packaging;
using System.Runtime.Serialization;
using System.IO.Compression;

namespace BLibrary.Util {
    public static class ZipUtils {
        public const string GZIP_SUFFIX = ".gz";

        public static void SaveToPackage (Package packed, string name, IFormatter formatter, Object obj) {
            PackagePart res = packed.CreatePart (PackUriHelper.CreatePartUri (new Uri ("/" + name, UriKind.Relative)), System.Net.Mime.MediaTypeNames.Application.Octet);
            using (Stream stream = new MemoryStream ()) {
                formatter.Serialize (stream, obj);
                stream.Flush ();
                stream.Position = 0;

                CopyStream (stream, res.GetStream ());
            }
        }

        public static void SaveToPackageAsText (Package packed, string name, string text) {
            PackagePart res = packed.CreatePart (PackUriHelper.CreatePartUri (new Uri ("/" + name, UriKind.Relative)), System.Net.Mime.MediaTypeNames.Text.Plain);
            using (Stream stream = new MemoryStream ()) {
                using (StreamWriter writer = new StreamWriter (stream)) {
                    writer.Write (text);
                    writer.Flush ();
                    stream.Position = 0;
                    CopyStream (stream, res.GetStream ());
                }
            }
        }

        public static Stream GetStream (Package packed, string name, IFormatter formatter) {
            PackagePart part = packed.GetPart (PackUriHelper.CreatePartUri (new Uri ("/" + name, UriKind.Relative)));
            return part.GetStream ();

        }

        public static void CopyStream (Stream source, Stream target) {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read (buf, 0, bufSize)) > 0)
                target.Write (buf, 0, bytesRead);
        }

        /// <summary>
        /// Compresses the given file, appending ".gz" as a suffix.
        /// </summary>
        /// <param name="file">File to compress.</param>
        /// <param name="cleanup">If set to <c>true</c> delete the original file on success.</param>
        public static void CompressFile (FileInfo file, bool cleanup) {
            using (FileStream originalFileStream = file.OpenRead ()) {
                using (FileStream compressedFileStream = File.Create (file.FullName + GZIP_SUFFIX)) {
                    using (GZipStream compressionStream = new GZipStream (compressedFileStream, CompressionMode.Compress)) {
                        originalFileStream.CopyTo (compressionStream);
                    }
                }
            }

            if (cleanup) {
                File.Delete (file.FullName);
            }
        }

        /// <summary>
        /// Decompresses the given file, removing the last extension.
        /// </summary>
        /// <param name="file">File to decompress.</param>
        /// <param name="cleanup">If set to <c>true</c> delete the original file on success.</param>
        public static void DecompressFile (FileInfo file, bool cleanup) {
            using (FileStream originalFileStream = file.OpenRead ()) {
                string currentFileName = file.FullName;
                string newFileName = currentFileName.Remove (currentFileName.Length - file.Extension.Length);

                using (FileStream decompressedFileStream = File.Create (newFileName)) {
                    using (GZipStream decompressionStream = new GZipStream (originalFileStream, CompressionMode.Decompress)) {
                        decompressionStream.CopyTo (decompressedFileStream);
                    }
                }
            }

            if (cleanup) {
                File.Delete (file.FullName);
            }
        }
    }
}

