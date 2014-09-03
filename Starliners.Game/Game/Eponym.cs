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

        #region Classes & Enums

        enum EponymStyle {
            Unknown,
            Single,
            Composite
        }

        abstract class NameSet {
            public abstract string GetRandomName (Random rand);
        }

        sealed class SingleNameSet : NameSet {

            List<string> _main = new List<string> ();

            public SingleNameSet (JsonArray arr) {
                foreach (string name in arr.GetEnumerable<string>()) {
                    _main.Add (name);
                }
            }

            public override string GetRandomName (Random rand) {
                return _main [rand.Next (_main.Count)];
            }
        }

        sealed class CompositeNameSet : NameSet {
            List<string> _prefix = new List<string> ();
            List<string> _main = new List<string> ();

            public CompositeNameSet (JsonArray prefix, JsonArray mains) {
                foreach (string name in prefix.GetEnumerable<string>()) {
                    _prefix.Add (name);
                }
                foreach (string name in mains.GetEnumerable<string>()) {
                    _main.Add (name);
                }
            }

            public override string GetRandomName (Random rand) {
                return string.Format ("{0} {1}", _prefix [rand.Next (_prefix.Count)], _main [rand.Next (_main.Count)]);
            }
        }

        #endregion

        List<string> _names = new List<string> ();
        Dictionary<string, NameSet> _namesets = new Dictionary<string, NameSet> ();

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

            return EnsureUniqueness (_names [Access.Seed.Next (_names.Count)]);
        }

        public string GenerateFleetName (Fleet fleet) {
            _fleetcount++;
            return string.Format ("{0} #{1}", fleet.Backer.FleetStyle, _fleetcount.ToString ());
        }

        public string GenerateShipName (ShipInstance ship) {
            if (_names.Count <= 0) {
                ReloadNames ();
            }

            return string.Format ("{0} {1}", ship.Origin.Owner.ShipPrefix, EnsureUniqueness (_namesets [ship.Origin.Owner.ShipStyle].GetRandomName (Access.Rand)));
        }

        string EnsureUniqueness (string name) {
            if (_used.ContainsKey (name)) {
                _used [name]++;
                name = string.Format ("{0} {1}", name, StringUtils.ToRomanNumeral (_used [name]));
            } else {
                _used [name] = 1;
            }

            return name;
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

            _namesets.Clear ();
            foreach (ResourceFile resource in GameAccess.Resources.Search("Data.Names.Ships")) {
                using (StreamReader reader = new StreamReader (resource.OpenRead ())) {
                    JsonNode json = JsonParser.JsonDecode (reader.ReadToEnd ());
                    foreach (JsonObject obj in json.AsEnumerable<JsonObject>()) {
                        string name = obj ["name"].GetValue<string> ();
                        EponymStyle style = obj.ContainsKey ("style") ? (EponymStyle)Enum.Parse (typeof(EponymStyle), obj ["style"].GetValue<string> (), true) : EponymStyle.Single;
                        NameSet nset;
                        if (style == EponymStyle.Single) {
                            nset = new SingleNameSet (obj ["list0"].GetValue<JsonArray> ());
                        } else {
                            nset = new CompositeNameSet (obj ["list0"].GetValue<JsonArray> (), obj ["list1"].GetValue<JsonArray> ());
                        }
                        _namesets [name] = nset;
                    }
                }
            }
        }

        public static Eponym GetForWorld (IWorldAccess access) {
            return access.States.Values.OfType<Eponym> ().First ();
        }
    }
}

