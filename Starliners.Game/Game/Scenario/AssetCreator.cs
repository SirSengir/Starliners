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

﻿using System.Collections.Generic;
using BLibrary.Resources;
using BLibrary.Json;
using System;

namespace Starliners.Game.Scenario {

    abstract class AssetCreator {
        #region Classes

        public abstract class ResourceParser {
            public string Ident { get; private set; }

            public string ResourcePattern { get; private set; }

            protected ResourceParser (string ident, string pattern) {
                Ident = ident;
                ResourcePattern = pattern;
            }

            public abstract void ParseResource (ParseableResource parseable, IWorldAccess access, IPopulator populator, AssetHolder holder);

            protected Dictionary<string, int> ParseProperties (IWorldAccess access, JsonArray table) {
                Dictionary<string, int> parsed = new Dictionary<string, int> ();
                foreach (JsonObject property in table.GetEnumerable<JsonObject>()) {
                    parsed [property ["key"].GetValue<string> ()] = (int)property ["value"].GetValue<double> ();
                }
                return parsed;
            }
        }

        public sealed class GenericParser : ResourceParser {

            Type _type;

            public GenericParser (string ident, string pattern, Type type)
                : base (ident, pattern) {
                _type = type;
            }

            public override void ParseResource (ParseableResource parseable, IWorldAccess access, IPopulator populator, AssetHolder holder) {
                foreach (JsonObject obj in parseable.Elements) {
                    string name = obj ["name"].GetValue<string> ();
                    holder.SetAsset (name, Activator.CreateInstance (_type, new object[] {
                        access,
                        name,
                        populator,
                        obj
                    }));
                }
            }

        }

        public sealed class PlainParser : ResourceParser {

            Type _type;

            public PlainParser (string ident, string pattern, Type type)
                : base (ident, pattern) {
                _type = type;
            }

            public override void ParseResource (ParseableResource parseable, IWorldAccess access, IPopulator populator, AssetHolder holder) {
                foreach (JsonObject obj in parseable.Elements) {
                    string name = obj ["name"].GetValue<string> ();
                    holder.SetAsset (name, Activator.CreateInstance (_type, new object[] {
                        obj
                    }));
                }
            }

        }

        #endregion

        #region Delegates

        protected delegate T ParseJsonValue<T> (JsonNode jsoned);

        #endregion

        public int Weight { get; private set; }

        public AssetCreator (int weight) {
            Weight = weight;
        }

        public abstract IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator);

        public virtual void OnCreated (IWorldAccess access, IPopulator populator) {
        }

        protected void PopulateFromResources (ResourceParser parser, IWorldAccess access, IPopulator populator, AssetHolder holder) {

            access.GameConsole.Info ("---------- Constructing {0} ----------", parser.Ident);

            foreach (ResourceFile resource in GameAccess.Resources.Search("Resources.Data." + parser.ResourcePattern)) {
                access.GameConsole.Debug ("Parsing file {0} for new {1}.", resource.Name, parser.Ident.ToLowerInvariant ());
                parser.ParseResource (new ParseableResource (resource), access, populator, holder);
            }

        }
    }

    sealed class AssetCreator<T> : AssetCreator where T : Asset {
        string _assetKey;
        string _ident;
        string _pattern;

        public AssetCreator (int weight, string ident, string pattern, string assetKey)
            : base (weight) {
            _ident = ident;
            _pattern = pattern;
            _assetKey = assetKey;
        }

        public override IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator) {
            AssetHolder<T> holder = new AssetHolder<T> (Weight, _assetKey);
            PopulateFromResources (new GenericParser (_ident, _pattern, typeof(T)), access, populator, holder);
            return new List<AssetHolder> { holder };
        }

        public override void OnCreated (IWorldAccess access, IPopulator populator) {
            base.OnCreated (access, populator);
            AssetHolder<T> holder = (AssetHolder<T>)populator.Holders [_assetKey];
            foreach (T asset in holder.GetEnumerable()) {
                asset.OnCreated (access, populator);
            }
        }
    }

    sealed class PlainCreator<T> : AssetCreator {
        string _assetKey;
        string _ident;
        string _pattern;

        public PlainCreator (int weight, string ident, string pattern, string assetKey)
            : base (weight) {
            _ident = ident;
            _pattern = pattern;
            _assetKey = assetKey;
        }

        public override IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator) {
            AssetHolder<T> holder = new AssetHolder<T> (Weight, _assetKey) { IsAsset = false };
            PopulateFromResources (new PlainParser (_ident, _pattern, typeof(T)), access, populator, holder);
            return new List<AssetHolder> { holder };
        }

    }
}

