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
using BLibrary.Json;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Starliners.Game.Planets;

namespace Starliners.Game {
    [Serializable]
    sealed class TriggerConditional : Trigger {

        List<Trigger> _triggers = new List<Trigger> ();
        bool _isOr;

        #region Constructor

        public TriggerConditional (IWorldAccess access, IPopulator populator, JsonObject json)
            : base (access) {

            _isOr = json.ContainsKey ("value") ? "OR".Equals (json ["value"].GetValue<string> ()) : false;
            if (json.ContainsKey ("trigger")) {
                foreach (JsonObject obj in json["trigger"].AsEnumerable<JsonObject>()) {
                    _triggers.Add (Trigger.InstantiateTrigger (access, populator, obj));
                }
            }
        }

        #endregion

        #region Serialization

        public TriggerConditional (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override bool IsTripped (ILevyProvider planet) {
            for (int i = 0; i < _triggers.Count; i++) {
                bool result = _triggers [i].IsTripped (planet);
                if (_isOr && result) {
                    return true;
                }
                if (!_isOr && !result) {
                    return false;
                }
            }
            return _isOr ? false : true;
        }

        public override bool IsTripped (Planet planet) {
            for (int i = 0; i < _triggers.Count; i++) {
                bool result = _triggers [i].IsTripped (planet);
                if (_isOr && result) {
                    return true;
                }
                if (!_isOr && !result) {
                    return false;
                }
            }
            return _isOr ? false : true;
        }
    }
}

