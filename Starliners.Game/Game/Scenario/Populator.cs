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
using System.Runtime.Serialization;
using System.Linq;
using Starliners.Game.Forces;
using Starliners.Game.Planets;
using Starliners.Game.Invasions;

namespace Starliners.Game.Scenario {
    [Serializable]
    sealed class Populator : IPopulator, ISerializable {
        public IReadOnlyDictionary<string, AssetHolder> Holders {
            get {
                return _holders;
            }
        }

        public AssetKeyMap KeyMap {
            get;
            private set;
        }

        public IMapGenerator MapGenerator {
            get;
            private set;
        }

        List<AssetCreator> _creators = new List<AssetCreator> ();
        Dictionary<string, AssetHolder> _holders = new Dictionary<string, AssetHolder> ();
        Blueprint _bpFleet;

        #region Constructor

        public Populator () {
            KeyMap = new AssetKeyMap ();
            Initialize ();
        }

        void Initialize () {
            _holders = new Dictionary<string, AssetHolder> ();

            int weight = 0;
            int step = 100;

            _creators.Add (new CreatorClassmaps (weight += step));
            _creators.Add (new CreatorBlueprints (weight += step));
            _creators.Add (new CreatorNotifications (weight += step));
            _creators.Add (new AssetCreator<Culture> (weight += step, "Cultures", "Cultures", AssetKeys.CULTURES));
            _creators.Add (new AssetCreator<FactionPreset> (weight += step, "Faction Presets", "Factions", AssetKeys.FACTION_PRESETS));
            _creators.Add (new PlainCreator<Improvement.Category> (weight += step, "Improvement Categories", "Improvements.Categories", AssetKeys.IMPROVEMENTS_CATEGORIES));
            _creators.Add (new AssetCreator<Improvement> (weight += step, "Improvements", "Improvements.Buildings", AssetKeys.IMPROVEMENTS));
            _creators.Add (new AssetCreator<ShipClass> (weight += step, "Ships", "Ships", AssetKeys.SHIPS));
            _creators.Add (new AssetCreator<Invader> (weight += step, "Invaders", "Invaders", AssetKeys.INVADERS));

            MapGenerator = new MapGenerator (this);
        }

        #endregion

        #region Serialization

        public Populator (SerializationInfo info, StreamingContext context) {

            if (info == null) {
                throw new ArgumentNullException ("info");
            }

            KeyMap = (AssetKeyMap)info.GetValue ("KeyMap", typeof(AssetKeyMap));
            Initialize ();
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("KeyMap", KeyMap, typeof(AssetKeyMap));
        }

        #endregion

        public Player CreatePlayer (IWorldAccess access, string login) {
            return new Player (access, login);
        }

        public EntityFleet CreateEntityFleet (IWorldAccess access, Fleet fleet) {
            return new EntityFleet (access, _bpFleet, fleet);
        }

        public IList<Asset> GetAssets (IWorldAccess access) {
            List<Asset> assets = new List<Asset> ();

            // First loop - create all AssetHolder
            foreach (AssetCreator creator in _creators.OrderBy(p => p.Weight)) {
                foreach (AssetHolder holder in creator.CreateAssets(access, this)) {
                    _holders [holder.Ident] = holder;
                }
            }
            foreach (AssetCreator creator in _creators.OrderBy(p => p.Weight)) {
                creator.OnCreated (access, this);
            }

            // Second loop - condense the collected AssetHolder into a simple list
            foreach (AssetHolder holder in _holders.Values.OrderBy(p => p.Weight)) {
                if (holder.IsAsset) {
                    foreach (Asset asset in holder.GetEnumerable<Asset>()) {
                        assets.Add (asset);
                    }
                }
            }

            _bpFleet = ((AssetHolder<Blueprint>)Holders [AssetKeys.BLUEPRINTS]).GetAsset ("fleet");
            return assets;
        }

        public IList<StateObject> CreateInitialStates (IWorldAccess access) {
            List<StateObject> initial = new List<StateObject> ();

            // Create empire factions
            AssetHolder<FactionPreset> presets = (AssetHolder<FactionPreset>)Holders [AssetKeys.FACTION_PRESETS];
            for (int i = 0; i < access.GetParameter<int> (ParameterKeys.EMPIRE_COUNT); i++) {
                string ident = "empire" + i.ToString ();
                initial.Add (new Faction (access, ident, presets.GetEnumerable ().Where (p => p.Flags.Contains (ident)).First ()) { IsPlayable = true });
            }

            foreach (Invader invader in access.Assets.Values.OfType<Invader>()) {
                initial.Add (invader.InitFaction (access));
            }

            initial.Add (new Eponym (access));
            initial.Add (new InvasionSpawner (access));
            initial.Add (new HistoryTracker (access));
            return initial;
        }
    }
}

