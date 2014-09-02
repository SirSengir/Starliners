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
using Starliners;

namespace BLibrary.Audio {

    sealed class SoundScenery {
        Dictionary<string, float> _volumes = new Dictionary<string, float> ();
        List<string> _silenced = new List<string> ();

        public void Clear () {
            _silenced.Clear ();
            foreach (string key in _volumes.Keys) {
                _silenced.Add (key);
            }
            _volumes.Clear ();
        }

        public void Add (string ident, float add, float max) {
            if (!_volumes.ContainsKey (ident)) {
                _volumes [ident] = 0;
                _silenced.Remove (ident);
            }

            if (_volumes [ident] + add < max) {
                _volumes [ident] += add;
            } else {
                _volumes [ident] = max;
            }
        }

        public bool IsMaxed (string ident, float max) {
            return _volumes.ContainsKey (ident) && _volumes [ident] >= max;
        }

        public void Update () {
            foreach (var entry in _volumes) {
                SoundManager.Instance.Start (entry.Key);
                SoundManager.Instance.ChangeVolume (entry.Key, entry.Value);
            }

            for (int i = 0; i < _silenced.Count; i++) {
                GameAccess.Interface.GameConsole.Audio ("Stopping scenery sound {0}.", _silenced [i]);
                SoundManager.Instance.Stop (_silenced [i]);
            }


        }
    }
}
