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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BLibrary.Json;
using Starliners;

namespace BLibrary.Resources {

    /// <summary>
    /// Assemblies or zip files able to provide ResourceFiles.
    /// </summary>
    public abstract class ResourceCollection : IComparable<ResourceCollection> {

        /// <summary>
        /// Whether or not the collection is enabled.
        /// </summary>
        public bool IsEnabled {
            get;
            set;
        }

        /// <summary>
        /// Whether or not this is a builtin collection which shall not be disabled.
        /// </summary>
        public bool IsBuiltin {
            get;
            private set;
        }

        /// <summary>
        /// Whether this collection can modify saves and save files need to be marked.
        /// </summary>
        public bool IsSaveModifier {
            get;
            private set;
        }

        /// <summary>
        /// Human-readable name of the collection.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Unique string identifier for the collection.
        /// </summary>
        public string UID {
            get;
            private set;
        }

        /// <summary>
        /// Author for the collection.
        /// </summary>
        public string Author {
            get;
            private set;
        }

        /// <summary>
        /// A description for the collection.
        /// </summary>
        public string Description {
            get;
            private set;
        }

        /// <summary>
        /// The collection's version.
        /// </summary>
        public Version Version {
            get;
            private set;
        }

        /// <summary>
        /// The collection's "weight".
        /// </summary>
        /// <remarks>Resources with the same identifier in heavier collections override those in lighter collections.</remarks>
        public int Weight {
            get;
            private set;
        }

        public string[] LocalizationResources {
            get;
            private set;
        }

        protected ResourceCollection (string name, Version version, int weight) {
            Name = name;
            UID = name;
            Version = version;
            Weight = weight;
            IsEnabled = true;
            IsSaveModifier = true;
        }

        /// <summary>
        /// Parse metadata from the plugin.
        /// </summary>
        protected void SetByMeta () {
            ResourceFile meta = SearchExact ("Meta.Plugin.json");
            if (meta == null) {
                return;
            }

            try {
                JsonObject table;
                using (StreamReader reader = new StreamReader (meta.OpenRead ())) {
                    table = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonObject> ();
                }

                Name = table ["name"].GetValue<string> ();
                UID = table ["uid"].GetValue<string> ();
                Description = table.ContainsKey ("author") ? table ["author"].GetValue<string> () : "<Unknown>";
                Description = table.ContainsKey ("description") ? table ["description"].GetValue<string> () : "<No description>";
                Weight = table.ContainsKey ("weight") ? (int)table ["weight"].GetValue<double> () : Weight;
                IsSaveModifier = table.ContainsKey ("save") ? table ["save"].GetValue<bool> () : true;
                IsBuiltin = table.ContainsKey ("builtin") ? table ["builtin"].GetValue<bool> () : false;

                if (table.ContainsKey ("version")) {
                    Version version;
                    if (Version.TryParse (table ["version"].GetValue<string> (), out version)) {
                        Version = version;
                    }
                }

                if (table.ContainsKey ("localizations")) {
                    IList<string> ress = new List<string> ();
                    foreach (JsonNode node in table["localizations"].GetValue<JsonArray>()) {
                        ress.Add (node.GetValue<string> ());
                    }
                    LocalizationResources = ress.ToArray ();
                } else
                    LocalizationResources = new string[0];

            } catch (Exception ex) {
                GameAccess.Simulator.GameConsole.Warning ("Failed to parse metadata from plugin {0}. Reason: {1}", Name, ex.Message);
                return;
            }

            // We know the true UID now, reset the enable/disable status.
            IsEnabled = GameAccess.Settings.HasKey ("plugins", UID) ? GameAccess.Settings.Get<bool> ("plugins", UID) : IsEnabled;

        }

        public abstract IEnumerable<ResourceFile> Search (string pattern);

        public abstract ResourceFile SearchExact (string pattern);

        public int CompareTo (ResourceCollection other) {
            if (Weight > other.Weight)
                return -1;
            if (Weight < other.Weight)
                return 1;

            return 0;
        }

    }

}
