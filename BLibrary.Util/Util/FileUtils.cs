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
using System.IO;
using System.Linq;
using System.Reflection;

namespace BLibrary.Util {

    public static class FileUtils {

        public static string CreateFileName (string raw) {
            foreach (char c in Path.GetInvalidFileNameChars()) {
                raw = raw.Replace (c, '_');
            }
            return raw;
        }

        public static bool IsValidFileName (string raw) {
            foreach (char c in Path.GetInvalidFileNameChars())
                if (raw.Contains (c))
                    return false;

            return true;
        }

        public static bool IsValidPathName (string raw) {
            foreach (char c in Path.GetInvalidPathChars())
                if (raw.Contains (c))
                    return false;

            return true;
        }

        public static IEnumerable<FileInfo> RetrieveAllMatching (DirectoryInfo root, string resourceident) {
            IEnumerable<FileInfo> allfiles = RetrieveAllFiles (root);
            return allfiles.Where (p => p.FullName.Replace ('/', '.').Contains (resourceident));
        }

        public static IEnumerable<FileInfo> RetrieveAllFiles (DirectoryInfo root) {
            List<FileInfo> files = new List<FileInfo> ();
            files.AddRange (root.GetFiles ());
            foreach (DirectoryInfo dir in root.GetDirectories()) {
                files.AddRange (RetrieveAllFiles (dir));
            }

            return files;
        }

        public static string GetAssemblyDirectory () {
            string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
            UriBuilder uri = new UriBuilder (codeBase);
            string path = Uri.UnescapeDataString (uri.Path);
            return Path.GetDirectoryName (path);
        }

        public static string NormalizePath (string path) {
            return Path.GetFullPath (new Uri (path).LocalPath)
                .TrimEnd (Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static bool AreMatchingDirectories (DirectoryInfo directory, DirectoryInfo other) {
            return string.Equals (NormalizePath (directory.FullName), NormalizePath (other.FullName));
        }

        /// <summary>
        /// Removes a file extension from the given string.
        /// </summary>
        /// <returns>The file extension.</returns>
        /// <param name="filename">Filename.</param>
        public static string RemoveFileExtension (string filename) {
            int fileExtPos = filename.LastIndexOf (".");
            return fileExtPos >= 0 ? filename.Substring (0, fileExtPos) : filename;
        }

        /// <summary>
        /// Merges two directories recursively.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void MergeDirectories (DirectoryInfo source, DirectoryInfo target) {

            if (source.FullName.ToLower () == target.FullName.ToLower ()) {
                return;
            }

            if (!Directory.Exists (source.FullName)) {
                Console.Out.WriteLine ("Source directory does not exist: " + source.FullName);
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (!Directory.Exists (target.FullName)) {
                Directory.CreateDirectory (target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo file in source.GetFiles()) {
                file.CopyTo (Path.Combine (target.ToString (), file.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory (diSourceSubDir.Name);
                MergeDirectories (diSourceSubDir, nextTargetSubDir);
            }
        }

        /// <summary>
        /// Compares the contents of two files by reading them and comparing their bytestreams.
        /// </summary>
        /// <returns><c>true</c>, if the file contents are identical, <c>false</c> otherwise.</returns>
        /// <param name="file">File.</param>
        /// <param name="other">Other.</param>
        public static bool AreIdenticalFiles (FileInfo file, FileInfo other) {

            // Determine if the same file was referenced two times.
            if (file == other) {
                return true;
            }

            // Open the two files.
            FileStream fs1 = new FileStream (file.FullName, FileMode.Open, FileAccess.Read);
            FileStream fs2 = new FileStream (other.FullName, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length) {
                // Close the file
                fs1.Close ();
                fs2.Close ();

                return false;
            }

            int file1byte;
            int file2byte;

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do {
                // Read one byte from each file.
                file1byte = fs1.ReadByte ();
                file2byte = fs2.ReadByte ();
            } while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close ();
            fs2.Close ();

            return ((file1byte - file2byte) == 0);
        }
    }
}
