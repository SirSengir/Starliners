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
using System.Linq;

namespace Starliners.Game {

    public abstract class AssetHolder {
        public string Ident { get; private set; }

        public int Weight { get; private set; }

        public bool IsAsset {
            get;
            set;
        }

        public IReadOnlyDictionary<string, object> Assets {
            get {
                return _assets;
            }
        }

        protected Dictionary<string, object> _assets = new Dictionary<string, object> ();

        public AssetHolder (int weight, string ident) {
            Weight = weight;
            Ident = ident;
            IsAsset = true;
        }

        public bool HasAsset (string key) {
            return _assets.ContainsKey (key);
        }

        public void SetAsset (string key, object asset) {
            _assets [key] = asset;
        }

        public T GetAsset<T> (string key) {
            if (!_assets.ContainsKey (key)) {
                throw new InvalidOperationException (string.Format ("Attempted to fetch an asset of the type {0} and the key {1}, but no such asset existed.", Ident, key));
            }
            return (T)_assets [key];
        }

        public T GetRandomAsset<T> (Random rand) {
            return (T)_assets.OrderBy (p => rand.Next ()).First ().Value;
        }

        public IEnumerable<T> GetEnumerable<T> () {
            return _assets.Values.Cast<T> ();
        }

        public void Append (IDictionary<string, object> added) {
            foreach (KeyValuePair<string, object> entry in added)
                _assets [entry.Key] = entry.Value;
        }
    }

    sealed class AssetHolder<T> : AssetHolder {
        public T this [string key] {
            get {
                return GetAsset (key);
            }
            set {
                _assets [key] = value;
            }
        }

        public AssetHolder (int weight, string ident)
            : base (weight, ident) {
        }

        public T GetAsset (string key) {
            return base.GetAsset<T> (key);
        }

        public T GetRandomAsset (Random rand) {
            return base.GetRandomAsset<T> (rand);
        }

        public IEnumerable<T> GetEnumerable () {
            return base.GetEnumerable<T> ();
        }
    }
}

