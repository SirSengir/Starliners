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
using System.Collections.Generic;
using System.IO;
using BLibrary.Json;

namespace BLibrary.Util {

    sealed class AssetLibrary {

        #region Properties

        public bool IsEmpty {
            get {
                return _fileAssetMap.Count <= 0;
            }
        }

        public IReadOnlyDictionary<string, FileAsset> FileAssetMap {
            get {
                return _fileAssetMap;
            }
        }

        public Version Version {
            get;
            private set;
        }

        #endregion

        Dictionary<string, FileAsset> _fileAssetMap = new Dictionary<string, FileAsset> ();

        public AssetLibrary(Version version) {
            Version = version;
        }

        public AssetLibrary (string json) {
            ParseFromJson (json);
        }

        /// <summary>
        /// Creates the asset library from the given file or an empty one, if the file does not exist or is unreadable.
        /// </summary>
        /// <param name="file"></param>
        public AssetLibrary (FileInfo file) {
            if (!file.Exists) {
                return;
            }

            string json = string.Empty;
            try {
                json = File.ReadAllText (file.FullName);
            } catch (Exception ex) {
                Console.Out.WriteLine ("Failed to read asset library: " + ex.Message);
            }

            if (string.IsNullOrWhiteSpace (json)) {
                return;
            }
            ParseFromJson (json);
        }

        void ParseFromJson (string json) {
            _fileAssetMap.Clear ();

            try {
                JsonObject result = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
                Version = result.ContainsKey("version") ? Version.Parse (result ["version"].GetValue<string> ()) : new Version();
                foreach (JsonObject parseable in result["assets"].AsEnumerable<JsonObject>()) {
                    FileAsset asset = new FileAsset (Version, parseable);
                    _fileAssetMap [asset.Ident] = asset;
                }
            } catch (Exception ex) {
                Console.Out.WriteLine ("Failed to parse asset library: " + ex.Message);
                _fileAssetMap.Clear ();
            }
        }

        /// <summary>
        /// Adds a new file asset to this library.
        /// </summary>
        /// <param name="asset">Asset.</param>
        public void AddAsset(FileAsset asset) {
            _fileAssetMap [asset.Ident] = asset;
        }

        /// <summary>
        /// Create a list of the assets which are outdated compared to the passed in asset library.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public IList<FileAsset> DetermineOutdatedAssets (AssetLibrary current) {
            IList<FileAsset> outdated = new List<FileAsset> ();

            foreach (KeyValuePair<string, FileAsset> entry in current.FileAssetMap) {
                if (!_fileAssetMap.ContainsKey (entry.Key)) {
                    outdated.Add (entry.Value);
                    continue;
                }

                if (_fileAssetMap [entry.Key].Version.CompareTo (entry.Value.Version) != 0) {
                    outdated.Add (entry.Value);
                }
            }

            return outdated;
        }

        /// <summary>
        /// Convert the asset library to a json string.
        /// </summary>
        /// <returns></returns>
        public string ToJson () {
            Hashtable sections = new Hashtable ();
            ArrayList files = new ArrayList ();

            foreach (KeyValuePair<string, FileAsset> entry in _fileAssetMap) {
                Hashtable filedef = new Hashtable ();
                filedef.Add ("file", entry.Key);
                if (entry.Value.AssetType != AssetType.Binary) {
                    filedef.Add ("type", entry.Value.AssetType.ToString ());
                }
                if (entry.Value.IsCompressed) {
                    filedef.Add ("compressed", entry.Value.IsCompressed);
                }
                if (entry.Value.NativeMap.Count > 0) {
                    Hashtable natives = new Hashtable ();
                    foreach(var nat in entry.Value.NativeMap) {
                        natives.Add (nat.Key.ToString ().ToLowerInvariant (), nat.Value);
                    }
                    filedef.Add ("natives", natives);
                }
                if (entry.Value.Version.CompareTo (Version) != 0) {
                    filedef.Add ("version", entry.Value.Version.ToString ());
                }
                files.Add (filedef);
            }

            sections.Add ("assets", files);
            sections.Add ("version", Version.ToString ());
            return JsonParser.JsonEncode (sections);
        }

        /// <summary>
        /// Write the asset library to the given path as a json file.
        /// </summary>
        /// <param name="file"></param>
        public void WriteToManifest (string file) {
            File.WriteAllText (file, ToJson ());
        }
    }
}
