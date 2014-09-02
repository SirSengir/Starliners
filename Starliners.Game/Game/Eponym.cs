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
using System.Collections.Generic;
using BLibrary.Serialization;
using BLibrary.Json;
using System.IO;
using BLibrary.Resources;
using BLibrary.Util;
using System.Linq;
using Starliners.Game.Forces;

namespace Starliners.Game {
    [Serializable]
    sealed class Eponym : StateObject {

        List<string> _names = new List<string> ();
        [GameData (Key = "Used")]
        Dictionary<string, int> _used = new Dictionary<string, int> ();
        [GameData (Key = "FleetCount")]
        int _fleetcount = 0;

        public Eponym (IWorldAccess access)
            : base (access, "eponym") {
        }

        #region Serialization

        public Eponym (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public string GeneratePlanetName () {
            if (_names.Count <= 0) {
                ReloadNames ();
            }

            string name = _names [Access.Seed.Next (_names.Count)];
            if (_used.ContainsKey (name)) {
                _used [name]++;
                name = string.Format ("{0} {1}", name, StringUtils.ToRomanNumeral (_used [name]));
            } else {
                _used [name] = 1;
            }

            return name;
        }

        public string GenerateFleetName (Fleet fleet) {
            _fleetcount++;
            return string.Format ("{0} #{1}", fleet.Backer.FleetStyle, _fleetcount.ToString ());
        }

        void ReloadNames () {
            _names.Clear ();
            foreach (ResourceFile resource in GameAccess.Resources.Search("Data.Names.Planets")) {
                using (StreamReader reader = new StreamReader (resource.OpenRead ())) {
                    JsonNode json = JsonParser.JsonDecode (reader.ReadToEnd ());
                    foreach (string str in json.AsEnumerable<string>()) {
                        _names.Add (str);
                    }
                }
            }
        }

        public static Eponym GetForWorld (IWorldAccess access) {
            return access.States.Values.OfType<Eponym> ().First ();
        }
    }
}

