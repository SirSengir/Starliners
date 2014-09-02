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
using BLibrary.Serialization;
using System.Collections.Generic;

namespace Starliners.Game.Planets {
    [Serializable]
    public sealed class Improvement : Asset {
        #region Constants

        const string NAME_PREFIX = "improvement";

        #endregion

        #region Classes

        [Serializable]
        public sealed class Category {
            public readonly string Name;
            public readonly int Weight;

            public Category (JsonObject json) {
                Name = json ["name"].GetValue<string> ();
                Weight = (int)json ["weight"].GetValue<double> ();
            }

        }

        #endregion

        [GameData (Remote = true)]
        public Category Categorized {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public string Icon {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public CostCalculator Cost {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Maximum {
            get;
            private set;
        }

        public IReadOnlyDictionary<string, int> Effects {
            get {
                return _effects;
            }
        }

        /// <summary>
        /// Flags set on this faction.
        /// </summary>
        public HashSet<string> Flags {
            get {
                return _flags;
            }
        }

        #region Fields

        List<Trigger> _triggers = new List<Trigger> ();
        Dictionary<string, int> _effects = new Dictionary<string, int> ();
        JsonObject _json;
        [GameData (Remote = true, Key = "Flags")]
        HashSet<string> _flags = new HashSet<string> ();

        #endregion

        public Improvement (IWorldAccess access, string name, IPopulator populator, JsonObject json)
            : base (access, Utils.BuildName (NAME_PREFIX, name), populator.KeyMap) {

            AssetHolder<Improvement.Category> categories = (AssetHolder<Improvement.Category>)populator.Holders [AssetKeys.IMPROVEMENTS_CATEGORIES];
            _json = json;
            Categorized = categories.GetAsset (json ["category"].GetValue<string> ());
            Icon = json.ContainsKey ("icon") ? json ["icon"].GetValue<string> () : "missing";
            Cost = json.ContainsKey ("cost") ? new CostCalculator (json ["cost"].GetValue<JsonObject> ()) : new CostCalculator (9999);
            Maximum = json.ContainsKey ("maximum") ? (int)json ["maximum"].GetValue<double> () : -1;

            if (json.ContainsKey ("effects")) {
                foreach (var entry in json["effects"].GetValue<JsonObject>()) {
                    _effects [entry.Key] = (int)entry.Value.GetValue<double> ();
                }
            }
            if (json.ContainsKey ("flags")) {
                foreach (string str in json["flags"].AsEnumerable<string>()) {
                    _flags.Add (str);
                }
            }

        }

        public override void OnCreated (IWorldAccess access, IPopulator populator) {
            base.OnCreated (access, populator);
            if (_json.ContainsKey ("trigger")) {
                foreach (JsonObject obj in _json["trigger"].AsEnumerable<JsonObject>()) {
                    _triggers.Add (Trigger.InstantiateTrigger (access, populator, obj));
                }
            }
        }

        #region Serialization

        public Improvement (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public bool IsAvailable (Planet planet) {
            for (int i = 0; i < _triggers.Count; i++) {
                if (!_triggers [i].IsTripped (planet)) {
                    return false;
                }
            }
            return true;
        }

    }
}

