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
using Starliners;

namespace BLibrary.Util {

    public sealed class DisposablesCache<T> where T : IDisposable {
        #region Index

        public T this [int hash] {
            get {
                return _cache [hash];
            }
            set {
                _cache [hash] = value;
            }
        }

        #endregion

        string _ident;
        int _maxCached = 5000;
        Dictionary<int, T> _cache = new Dictionary<int, T> ();

        #region Constructor

        public DisposablesCache (string ident) {
            _ident = ident;
        }

        #endregion

        public bool HasCached (int hash) {
            return _cache.ContainsKey (hash);
        }

        public void Remove (int hash) {
            if (!_cache.ContainsKey (hash)) {
                return;
            }

            Console.Out.WriteLine ("Removing a {0} from cache '{1}' (CacheCode: {2}).", GetType ().GetGenericArguments () [0], _ident, hash);
            _cache [hash].Dispose ();
            _cache.Remove (hash);
        }

        public void Maintain () {
            if (_cache.Count < _maxCached) {
                return;
            }

            Cleanup ();
        }

        public void Cleanup () {
            GameAccess.Interface.GameConsole.Debug ("Emptying cache buffer '{0}'.", _ident);

            foreach (var entry in _cache) {
                entry.Value.Dispose ();
            }
            _cache.Clear ();
        }
    }
}

