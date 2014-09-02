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

﻿using Starliners.Map;

namespace Starliners.States {

    sealed class StageReceiveSetup : LoadStage {
        float _percent;

        public override float Percent {
            get {
                return _percent;
            }
        }

        public override string Status {
            get {
                return _percent >= 1 ?
                    "Decorating the queen's nest..." : _percent > 0 ?
                    "Furnishing the hive..." : "Waiting on worker bees...";
            }
        }

        LoadState _state;

        public StageReceiveSetup (LoadState state) {
            _state = state;
        }

        public override void ThreadStart () {
            while (!WorldInterface.Instance.HasGameData) {
                //if (GameAccess.Interface.Networking != null && GameAccess.Interface.Networking.BytesExpected > 0) {
                //    _percent = (float)GameAccess.Interface.Networking.BytesReceived / GameAccess.Interface.Networking.BytesExpected;
                //} else {
                _percent = 0f;
                //}
                System.Threading.Thread.Sleep (100);
            }
            IsComplete = true;
        }

        public override void End () {
            base.End ();
            _state.NextState.Map.Control = new ControlPlayer (WorldInterface.Instance.ThePlayer);
        }
    }
}
