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
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BLibrary.Json;
using BLibrary.Util;

namespace BLauncher {

    sealed class LauncherSettings {
        public string Login {
            get;
            set;
        }

        public string Password {
            get;
            set;
        }

        public bool Remember {
            get;
            set;
        }

        public string LastChannel {
            get;
            set;
        }

        byte[] _kryptoKey;
        byte[] _kryptoIv;
        TripleDESCryptoServiceProvider _crypto;
        FileInfo _settingfile;
        FolderManager _folders;

        /// <summary>
        /// Creates the asset library from the given file or an empty one, if the file does not exist or is unreadable.
        /// </summary>
        /// <param name="file"></param>
        public LauncherSettings (FolderManager folders)
            : this () {
            _folders = folders;
            _settingfile = _folders [LauncherConstants.PATH_ROOT, "Launcher.json"];
            if (_settingfile.Exists) {
                string json = string.Empty;
                try {
                    json = File.ReadAllText (_settingfile.FullName);
                } catch (Exception ex) {
                    Console.Out.WriteLine ("Failed to read launcher settings: " + ex.Message);
                }

                if (string.IsNullOrWhiteSpace (json)) {
                    return;
                }
                ParseFromJson (json);
            }
        }

        LauncherSettings () {
            _kryptoKey = System.Text.ASCIIEncoding.ASCII.GetBytes ("GSYAHAGCBDUUADIADKOPAAAW");
            _kryptoIv = System.Text.ASCIIEncoding.ASCII.GetBytes ("USAZBGAW");
            _crypto = new TripleDESCryptoServiceProvider ();
        }

        void ParseFromJson (string json) {
            try {
                JsonObject result = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
                Login = result.ContainsKey ("login") ? result ["login"].GetValue<string> () : string.Empty;
                try {
                    Password = result.ContainsKey ("password") ? Decrypt (result ["password"].GetValue<string> ()) : string.Empty;
                } catch (Exception ex) {
                    Console.Out.WriteLine ("Decrypting password failed: " + ex.Message);
                }
                Remember = result.ContainsKey ("remember") ? result ["remember"].GetValue<bool> () : false;
                LastChannel = result.ContainsKey ("channel") ? result ["channel"].GetValue<string> () : string.Empty;
            } catch (Exception ex) {
                Console.Out.WriteLine ("Launcher settings were invalid. Ignoring. " + ex.Message);
            }
        }

        string ToJson () {
            Hashtable sections = new Hashtable ();
            sections.Add ("login", Login);
            sections.Add ("password", Encrypt (Password));
            sections.Add ("remember", Remember);
            sections.Add ("channel", LastChannel);
            return JsonParser.JsonEncode (sections);
        }

        /// <summary>
        /// Write the asset library to the given path as a json file.
        /// </summary>
        /// <param name="file"></param>
        public void WriteToFile () {
            File.WriteAllText (_settingfile.FullName, ToJson ());
        }

        #region Encryption

        string Encrypt (string text) {
            return Transform (text, _crypto.CreateEncryptor (_kryptoKey, _kryptoIv));
        }

        string Decrypt (string encrypted) {
            return Transform (encrypted, _crypto.CreateDecryptor (_kryptoKey, _kryptoIv));
        }

        string Transform (string text, ICryptoTransform transform) {
            if (text == null) {
                return null;
            }

            using (MemoryStream stream = new MemoryStream ()) {
                using (CryptoStream cryptoStream = new CryptoStream (stream, transform, CryptoStreamMode.Write)) {
                    byte[] input = Encoding.Unicode.GetBytes (text);
                    cryptoStream.Write (input, 0, input.Length);
                    cryptoStream.FlushFinalBlock ();

                    return Encoding.Unicode.GetString (stream.ToArray ());
                }
            }
        }

        #endregion
    }
}
