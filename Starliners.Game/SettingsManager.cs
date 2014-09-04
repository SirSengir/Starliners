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

﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using BLibrary.Json;

namespace Starliners {

    public sealed class SettingsManager {
        string _filepath;
        Dictionary<string, Dictionary<string, object>> _settings = new Dictionary<string, Dictionary<string, object>> ();

        public SettingsManager (string file) {
            _filepath = GameAccess.Folders.GetFilePath (Constants.PATH_SETTINGS, file);
            CreateDefaultIfNeeded (_filepath);
            if (ValidateFile (_filepath))
                ParseSettings (_filepath);
        }

        void CreateDefaultIfNeeded (string filepath) {
            if (ValidateFile (filepath))
                return;

            string defaultsettings = @"{
    ""profile"": [
        {
            ""login"": ""ThePlayer""
        }
    ],
    ""video"": [
        {
            ""key"": ""mode"",
            ""value"": ""fullscreen""
        },
        {
            ""key"": ""resolution"",
            ""value"": ""1920x1080""
        },
        {
            ""key"": ""shadows"",
            ""value"": true
        }
    ]
    ""sound"": [
        {
            ""key"": ""effects"",
            ""value"": true
        },
        {
            ""key"": ""music"",
            ""value"": true
        },
    ]
}";
            File.WriteAllText (filepath, defaultsettings);
        }

        bool ValidateFile (string filepath) {
            return File.Exists (filepath);
        }

        void ParseSettings (string filepath) {
            string json = File.ReadAllText (filepath);
            JsonObject result = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
            foreach (KeyValuePair<string, JsonNode> entry in result) {
                string section = entry.Key;
                if (!_settings.ContainsKey (section)) {
                    _settings [section] = new Dictionary<string, object> ();
                }

                foreach (var setting in entry.Value.AsEnumerable<JsonObject>()) {
                    _settings [section] [setting ["key"].GetValue<string> ()] = setting ["value"].GetValue<object> ();
                }
            }
        }

        /// <summary>
        /// Determines whether the given key is present in the given section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey (string section, string key) {
            if (!_settings.ContainsKey (section))
                return false;
            return _settings [section].ContainsKey (key);
        }

        public T Get<T> (string section, string key) {
            if (!_settings.ContainsKey (section))
                return default(T);
            if (!_settings [section].ContainsKey (key))
                return default(T);
            return (T)_settings [section] [key];
        }

        public void Set (string section, string key, object value) {
            if (!_settings.ContainsKey (section))
                _settings [section] = new Dictionary<string, object> ();

            _settings [section] [key] = value;
        }

        public void Flush () {
            Hashtable sections = new Hashtable ();
            foreach (KeyValuePair<string, Dictionary<string, object>> section in _settings) {
                ArrayList settings = new ArrayList ();
                foreach (KeyValuePair<string, object> entry in section.Value) {
                    Hashtable setting = new Hashtable ();
                    setting.Add ("key", entry.Key);
                    setting.Add ("value", entry.Value);
                    settings.Add (setting);
                }
                sections.Add (section.Key, settings);
            }

            string json = JsonParser.JsonEncode (sections);
            File.WriteAllText (_filepath, json);
        }
    }
}
