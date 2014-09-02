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
using System.Linq;
using System.Collections.Generic;

namespace Starliners.Game.Invasions {
    [Serializable]
    sealed class Invader : Asset {
        #region Constants

        const string NAME_PREFIX = "invader";

        #endregion

        internal WaveDefinition this [int key] {
            get {
                return _waves [key];
            }
        }

        public Faction Faction {
            get {
                if (_faction == null) {
                    _faction = (Faction)Access.States.Values.Where (p => string.Equals (p.Name, Utils.BuildName (Faction.NAME_PREFIX, NAME_PREFIX, Serial.ToString ()))).First ();
                }
                return _faction;
            }
        }

        public Culture Culture {
            get;
            private set;
        }

        FactionPreset _preset;
        Faction _faction;
        Dictionary<int, WaveDefinition> _waves = new Dictionary<int, WaveDefinition> ();

        public Invader (IWorldAccess access, string name, IPopulator populator, JsonObject json)
            : base (access, Utils.BuildName (NAME_PREFIX, name), populator.KeyMap) {
            AssetHolder<FactionPreset> presets = (AssetHolder<FactionPreset>)populator.Holders [AssetKeys.FACTION_PRESETS];
            AssetHolder<Culture> cultures = (AssetHolder<Culture>)populator.Holders [AssetKeys.CULTURES];

            _preset = presets [json ["faction"].GetValue<string> ()];
            Culture = cultures [json ["culture"].GetValue<string> ()];

            foreach (JsonObject wave in json["waves"].AsEnumerable<JsonObject>()) {
                WaveDefinition def = new WaveDefinition (wave);
                _waves [def.Id] = def;
            }
        }

        #region Serialization

        public Invader (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public Faction InitFaction (IWorldAccess access) {
            return new Faction (access, Utils.BuildName (NAME_PREFIX, Serial.ToString ()), _preset);
        }

        public int GetWave (int waveCount) {
            return _waves.Where (p => p.Value.IsActive (waveCount)).OrderBy (p => Access.Rand.Next ()).FirstOrDefault ().Value.Id;
        }
    }
}

