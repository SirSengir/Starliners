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
using System.Net;
using BLibrary.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Starliners.Network {
    public sealed class ServerCache {

        #region Constants

        const string CACHE_FILE = "ServerCache.json";

        #endregion

        #region Classes & Enums

        public enum ServerStatus {
            Unknown,
            Online,
            Offline
        }

        public sealed class ServerInfo {

            public ServerStatus Status {
                get;
                set;
            }

            public string Description {
                get;
                set;
            }

            public Version Version {
                get;
                set;
            }

            public long LastConnection {
                get;
                set;
            }

            public readonly IPAddress IPAddress;
            public readonly int Port;

            public ServerInfo (IPAddress address, int port) {
                IPAddress = address;
                Port = port;
                Version = new Version ();
            }

            public ServerInfo (JsonObject json) {
                IPAddress = IPAddress.Parse (json ["ip"].GetValue<string> ());
                Port = json.ContainsKey ("port") ? (int)json ["port"].GetValue<double> () : 11000;
                Description = json.ContainsKey ("description") ? json ["description"].GetValue<string> () : string.Empty;
                Version = json.ContainsKey ("version") ? Version.Parse (json ["version"].GetValue<string> ()) : new Version ();
            }

            internal Hashtable AsHashtable () {
                Hashtable table = new Hashtable ();

                table ["ip"] = IPAddress.ToString ();
                table ["port"] = Port;
                if (!string.IsNullOrEmpty (Description)) {
                    table ["description"] = Description;
                }
                table ["version"] = Version.ToString ();

                return table;
            }
        }

        #endregion

        #region Properties

        public ServerInfo this [IPAddress address, int port] {
            get {
                if (!_servers.Any (p => p.Port == port && p.IPAddress.Equals (address))) {
                    _servers.Add (new ServerInfo (address, port));
                }
                return _servers.First (p => p.Port == port && p.IPAddress.Equals (address));
            }
        }

        public IReadOnlyList<ServerInfo> Servers {
            get {
                return _servers;
            }
        }

        #endregion

        List<ServerInfo> _servers = new List<ServerInfo> ();

        public ServerCache () {
            string filepath = GameAccess.Folders.GetFilePath (Constants.PATH_SETTINGS, CACHE_FILE);
            if (File.Exists (filepath)) {
                ParseCache (filepath);
            }
        }

        void ParseCache (string filepath) {
            try {

                JsonArray result = JsonParser.JsonDecode (File.ReadAllText (filepath)).GetValue<JsonArray> ();

                foreach (JsonObject json in result.GetEnumerable<JsonObject>()) {
                    _servers.Add (new ServerInfo (json));
                }

            } catch (Exception ex) {
                Console.Out.WriteLine ("Ignored broken server cache: " + ex.Message);
            }
        }

        public void Flush () {
            ArrayList infos = new ArrayList ();
            foreach (ServerInfo server in _servers) {
                infos.Add (server.AsHashtable ());
            }

            string json = JsonParser.JsonEncode (infos);
            File.WriteAllText (GameAccess.Folders.GetFilePath (Constants.PATH_SETTINGS, CACHE_FILE), json);
        }

    }
}

