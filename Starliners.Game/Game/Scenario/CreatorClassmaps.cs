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
using BLibrary.Json;
using BLibrary.Util;
using System.Reflection;

namespace Starliners.Game.Scenario {
    sealed class CreatorClassmaps : AssetCreator {

        #region Classes

        sealed class ClassmapParser : ResourceParser {
            readonly List<Assembly> _assemblies = new List<Assembly> ();

            public ClassmapParser (string ident, string pattern)
                : base (ident, pattern) {

                _assemblies.Add (ReflectionUtils.SearchAssembly (Constants.AssemblySimulator));
            }

            public override void ParseResource (ParseableResource parseable, IWorldAccess access, IPopulator populator, AssetHolder holder) {
                AssetHolder<Type> classmaps = (AssetHolder<Type>)holder;

                foreach (JsonObject table in parseable.Elements) {
                    string category = table ["category"].GetValue<string> ();
                    string key = table ["key"].GetValue<string> ();
                    string ident = string.Format ("{0}.{1}", category, key);
                    if (holder.HasAsset (ident)) {
                        throw new ParsingFailedException (parseable, "Cannot redefine a classmap with the key: " + ident);
                    }

                    string type = table ["type"].GetValue<string> ();
                    Type typ = SearchInAssemblies (type);
                    if (typ == null) {
                        throw new ParsingFailedException (parseable, "Unable to find a matching type for the string {0}.", type);
                    }
                    classmaps [ident] = typ;
                }
            }

            Type SearchInAssemblies (string type) {
                Type typ = Type.GetType (type);

                if (typ == null) {
                    for (int i = 0; i < _assemblies.Count; i++) {
                        typ = _assemblies [i].GetType (type);
                        if (typ != null) {
                            break;
                        }
                    }
                }

                return typ;
            }
        }

        #endregion

        public CreatorClassmaps (int weight)
            : base (weight) {
        }

        public override IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator) {
            AssetHolder<Type> classmaps = new AssetHolder<Type> (Weight, AssetKeys.CLASSMAPS) { IsAsset = false };
            PopulateFromResources (new ClassmapParser ("Classmaps", "Classmaps"), access, populator, classmaps);
            return new List<AssetHolder> () { classmaps };
        }
    }
}

