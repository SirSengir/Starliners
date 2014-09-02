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
using BLibrary.Serialization;
using System.Collections.Generic;
using BLibrary.Util;
using System.Linq;

namespace Starliners.Game.Invasions {
    [Serializable]
    sealed class InvasionSpawner : StateObject {

        [GameData (Key = "NextInvasion")]
        long _nextInvasion;
        [GameData (Key = "InvasionCount")]
        int _invasionCount;

        public InvasionSpawner (IWorldAccess access)
            : base (access, "InvasionSpawner") {
            IsTickable = true;
            _nextInvasion = access.Clock.Ticks + 200;
        }

        #region Serialization

        public InvasionSpawner (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            if (Access.Clock.Ticks < _nextInvasion) {
                return;
            }
            _nextInvasion = Access.Clock.Ticks + 2000;
            _invasionCount++;

            Invader invader = Access.Assets.Values.OfType<Invader> ().OrderBy (p => Access.Seed.Next ()).First ();
            InvasionBacker backer = new InvasionBacker (Access, "wave_" + _invasionCount.ToString (), invader, _invasionCount);
            Access.Controller.QueueState (backer);
        }

    }
}

