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
using BLibrary.Util;
using BLibrary.Serialization;
using System.Collections.Generic;
using System.Linq;
using Starliners.Game.Forces;
using Starliners.Game.Planets;

namespace Starliners.Game.Invasions {
    [Serializable]
    sealed class InvasionBacker : StateObject, ILevyProvider, IFleetBacker {

        public Faction FleetOwner {
            get {
                return Owner;
            }
        }

        public Faction Owner {
            get {
                return _invader.Faction;
            }
        }

        public ColourScheme Colours {
            get {
                return FleetOwner.Colours;
            }
        }

        public string FleetStyle {
            get {
                return FleetOwner.FleetStyle;
            }
        }

        public Culture Culture {
            get {
                return _invader.Culture;
            }
        }

        public uint Reenforcement {
            get {
                return 0;
            }
        }

        [GameData (Remote = true, Key = "Invader")]
        Invader _invader;
        [GameData (Key = "Wave")]
        int _wave;
        [GameData (Key = "WaveCount")]
        int _waveCount;
        [GameData (Key = "Levy")]
        Levy _levy;
        [GameData (Key = "Fleet")]
        Fleet _fleet;

        [GameData (Key = "InvasionSetup")]
        bool _invasionSetup;
        [GameData (Key = "FleetLaunched")]
        bool _fleetLaunched;

        public InvasionBacker (IWorldAccess access, string name, Invader invader, int waveCount)
            : base (access, name) {
            IsTickable = true;
            _invader = invader;
            _wave = invader.GetWave (waveCount);
            _waveCount = waveCount;
        }

        #region Serialization

        public InvasionBacker (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            if (!_invasionSetup) {
                _levy = new Levy (Access, Name + "_levy0", this);
                Access.Controller.QueueState (_levy);

                _fleet = _levy.Raise (GetRandomStartingLocation (), this);
                Access.Controller.QueueState (_fleet);
                _invasionSetup = true;

            } else if (!_fleetLaunched) {
                _levy.Rejuvenate ();
                _fleet.Relocate (Access.Entities.Values.OfType<EntityPlanet> ().OrderBy (p => Access.Seed.Next ()).First ());
                _fleetLaunched = true;
            }
        }

        public int GetMaintenance (ShipSize size) {
            return _invader [_wave].GetMaintenance (size, _waveCount);
        }

        public ShipModifiers CreateShipModifiers (ShipSize size) {
            return _invader [_wave].CreateShipModifiers (size);
        }

        public int GetAttribute (string key) {
            return _invader [_wave].GetAttribute (key);
        }

        public void OnFleetDisolution (Fleet fleet) {
            IsDead = true;
            _fleet = null;
            _levy.Disband ();
            _levy = null;
        }

        public void OnShipLoss (ShipClass sclass) {
        }

        Vect2d GetRandomStartingLocation () {
            int planetcount = Access.GetParameter<int> (ParameterKeys.EMPIRE_SIZE);
            int delta = (int)Math.Sqrt (planetcount) * 8;
            int minX = -delta / 2, minY = -delta / 2;

            return new Vect2d (minX, minY) * (Access.CoinToss ? 1 : -1);
        }

    }
}

