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
using BLibrary.Json;
using BLibrary.Util;
using System.Collections.Generic;
using BLibrary.Serialization;

namespace Starliners.Game {
    [Serializable]
    public sealed class FactionPreset : Asset {

        #region Constants

        const string NAME_PREFIX = "fpreset";

        #endregion

        [GameData (Remote = true)]
        public string FullName {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public ColourScheme Colours {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public Blazon Blazon {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public string FleetStyle {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public string FleetIcons {
            get;
            private set;
        }

        /// <summary>
        /// Flags set on this faction.
        /// </summary>
        public HashSet<string> Flags {
            get {
                return _flags;
            }
        }

        public IReadOnlyList<Culture> Cultures {
            get {
                return _cultures;
            }
        }

        [GameData (Remote = true)]
        HashSet<string> _flags = new HashSet<string> ();
        [GameData (Remote = true)]
        List<Culture> _cultures = new List<Culture> ();

        public FactionPreset (IWorldAccess access, string name, IPopulator populator, JsonObject json)
            : base (access, Utils.BuildName (NAME_PREFIX, name), populator.KeyMap) {
            AssetHolder<Culture> cultures = (AssetHolder<Culture>)populator.Holders [AssetKeys.CULTURES];

            FullName = json.ContainsKey ("fullName") ? json ["fullName"].GetValue<string> () : "Faction";
            Colours = json.ContainsKey ("colours") ? new ColourScheme (json ["colours"].GetValue<JsonObject> ()) : new ColourScheme (Colour.LightGray, Colour.LightGray, Colour.LightGray, Colour.LightGray);
            Blazon = json.ContainsKey ("blazon") ? new Blazon (access, json ["blazon"].GetValue<JsonObject> ()) : Blazon.CreateRandom (access, BlazonShape.Shield);
            FleetStyle = json.ContainsKey ("fleetStyle") ? json ["fleetStyle"].GetValue<string> () : "Imperial Fleet";
            FleetIcons = json.ContainsKey ("fleetIcons") ? json ["fleetIcons"].GetValue<string> () : "fleet";

            if (json.ContainsKey ("flags")) {
                foreach (string str in json["flags"].AsEnumerable<string>()) {
                    _flags.Add (str);
                }
            }
            foreach (string str in json["cultures"].AsEnumerable<string>()) {
                _cultures.Add (cultures [str]);
            }
        }

        #region Serialization

        public FactionPreset (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

    }
}

