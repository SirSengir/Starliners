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
using System.Runtime.Serialization;

namespace BLibrary.Saves {
    [Serializable]
    public struct SaveHeader : ISerializable {
        /// <summary>
        /// Version of the game this save was created with.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Save format. (Smaller is older.)
        /// </summary>
        public int Format { get; private set; }

        /// <summary>
        /// Hash of the game this save was created with.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Name of the save.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// File name.
        /// </summary>
        public string File { get; private set; }

        /// <summary>
        /// Current elapsed ticks in the saved world.
        /// </summary>
        public long Ticks { get; private set; }

        /// <summary>
        /// Information on the plugins active and relevant when the save was created.
        /// </summary>
        public PluginInfo[] Plugins { get; private set; }

        public SaveHeader (Version version, string hash, string name, string file, long ticks, params PluginInfo[] plugins)
            : this () {
            Version = version;
            Format = 1;
            Hash = hash;
            Name = name;
            File = file;
            Ticks = ticks;
            Plugins = plugins;
        }

        #region Serialization

        public SaveHeader (SerializationInfo info, StreamingContext context)
            : this () {
            Version = Version.Parse (info.GetString ("Version"));
            Format = info.GetInt32 ("Format");
            Hash = info.GetString ("Hash");
            Name = info.GetString ("Name");
            File = info.GetString ("File");
            Ticks = info.GetInt64 ("Ticks");
            Plugins = (PluginInfo[])info.GetValue ("Plugins", typeof(PluginInfo[]));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Version", Version.ToString ());
            info.AddValue ("Format", Format);
            info.AddValue ("Hash", Hash);
            info.AddValue ("Name", Name);
            info.AddValue ("File", File);
            info.AddValue ("Ticks", Ticks);
            info.AddValue ("Plugins", Plugins, typeof(PluginInfo[]));
        }

        #endregion

        public override string ToString () {
            return string.Format ("[SaveHeader: Version={0}, Format={1}, Hash={2}, Name={3}, File={4}, Ticks={5}, Plugins.Length={6}]", Version, Format, Hash, Name, File, Ticks, Plugins.Length);
        }
    }

}

