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
using BLibrary.Json;
using BLibrary.Util;
using System.IO;

namespace BLibrary.Util {

    enum AssetType : byte {
        Binary,
        Plugin
    }

    /// <summary>
    /// Simple class combining the information on a file.
    /// </summary>
    sealed class FileAsset {

        #region Constants

        const string PATH_SUB_BINARY = "Bin/";
        const string PATH_SUB_PLUGINS = "Plugins/";

        #endregion

        #region Properties

        public string Ident {
            get;
            private set;
        }

        public AssetType AssetType {
            get;
            private set;
        }

        public Version Version {
            get;
            private set;
        }

        public bool IsCompressed {
            get;
            private set;
        }

        public IReadOnlyDictionary<PlatformOS, string> NativeMap {
            get { return _osFileMap; }
        }
        #endregion

        Dictionary<PlatformOS, string> _osFileMap = new Dictionary<PlatformOS, string> ();
        bool _isNative = false;

        #region Constructor

        public FileAsset(string ident, AssetType type, Version version, bool compressed, IReadOnlyDictionary<PlatformOS, string> nativeMap) {
            Ident = ident;
            AssetType = type;
            Version = version;
            IsCompressed = compressed;

            if (nativeMap.Count > 0) {
                _isNative = true;
                foreach (var entry in nativeMap) {
                    _osFileMap [entry.Key] = entry.Value;
                }
            }
        }

        public FileAsset (Version manifestVersion, JsonObject json) {
            Ident = json ["file"].GetValue<string> ();
            AssetType = json.ContainsKey ("type") ? (AssetType)Enum.Parse (typeof(AssetType), json ["type"].GetValue<string> (), true) : AssetType.Binary;
            Version = json.ContainsKey ("version") ? Version.Parse (json ["version"].GetValue<string> ()) : manifestVersion;
            IsCompressed = json.ContainsKey ("compressed") ? json ["compressed"].GetValue<bool> () : false;

            _isNative = json.ContainsKey ("natives");
            if (_isNative) {
                JsonObject natives = json ["natives"].GetValue<JsonObject> ();
                foreach (PlatformOS os in Enum.GetValues(typeof(PlatformOS))) {
                    if (natives.ContainsKey (os.ToString ().ToLowerInvariant ())) {
                        _osFileMap [os] = natives [os.ToString ().ToLowerInvariant ()].GetValue<string> ();
                    }
                }
            }
        }

        #endregion

        string GetSubDir () {
            return AssetType == AssetType.Binary ? "Bin/" : "Plugins/";
        }

        /// <summary>
        /// Determines whether the file has to be downloaded for the given platform.
        /// </summary>
        /// <param name="os"></param>
        /// <returns></returns>
        public bool MustDownload (PlatformOS os) {
            return !_isNative || _osFileMap.ContainsKey (os);
        }

        public IList<FileInfo> GetLocalSources(DirectoryInfo source) {
            if (NativeMap.Count <= 0) {
                return new List<FileInfo> () { new FileInfo(Path.Combine(source.FullName, Ident)) };
            }

            List<FileInfo> list = new List<FileInfo> ();
            foreach (var entry in NativeMap) {
                list.Add (new FileInfo (Path.Combine (source.FullName, entry.Value)));
            }
            return list;
        }

        public IList<FileInfo> GetLocalTargets(DirectoryInfo target, bool assumeCompressed) {
            if (NativeMap.Count <= 0) {
                string file = assumeCompressed ? Ident + ZipUtils.GZIP_SUFFIX : Ident;
                return new List<FileInfo> () { new FileInfo(Path.Combine(target.FullName, file)) };
            }

            List<FileInfo> list = new List<FileInfo> ();
            foreach (var entry in NativeMap) {
                string file = assumeCompressed ? entry.Value + ZipUtils.GZIP_SUFFIX : entry.Value;
                list.Add (new FileInfo (Path.Combine (target.FullName, "natives/" + entry.Key.ToString ().ToLowerInvariant () + "/" + file)));
            }
            return list;
        }

        /// <summary>
        /// Returns the download source for the given OS.
        /// </summary>
        /// <param name="os"></param>
        /// <returns></returns>
        public string GetDownloadSource (PlatformOS os) {
            string source = _osFileMap.ContainsKey (os) ? "natives/" + os.ToString ().ToLowerInvariant () + "/" + _osFileMap [os] : Ident;
            return IsCompressed ? source + ZipUtils.GZIP_SUFFIX : source;
        }

        /// <summary>
        /// Returns the download target for the given OS.
        /// </summary>
        /// <param name="os"></param>
        /// <returns></returns>
        public string GetDownloadTarget (PlatformOS os) {
            string target = GetSubDir () + (_osFileMap.ContainsKey (os) ? _osFileMap [os] : Ident);
            return IsCompressed ? target + ZipUtils.GZIP_SUFFIX : target;
        }

        public override string ToString () {
            return string.Format ("[FileAsset: Ident={0}, AssetType={1}, Version={2}, IsCompressed={3}, Native={4}]", Ident, AssetType, Version, IsCompressed, NativeMap.Count > 0);
        }
    }

}
