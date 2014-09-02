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
using System.Collections.Generic;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class ShipReport : ISerializable {

        #region Constants

        public const string ATTACK_HEAT_RESISTED = "HeatResisted";
        public const string ATTACK_HEAT_DELIVERED = "HeatDelivered";
        public const string ATTACK_KINETIC_RESISTED = "KineticResisted";
        public const string ATTACK_KINETIC_DELIVERED = "KineticDelivered";
        public const string ATTACK_RADIATION_RESISTED = "RadiationResisted";
        public const string ATTACK_RADIATION_DELIVERED = "RadiationDelivered";

        public const string DMG_SHIELD_RESISTED = "ShieldResisted";
        public const string DMG_SHIELD_DAMAGE = "ShieldDamage";
        public const string DMG_ARMOUR_RESISTED = "ArmourResisted";
        public const string DMG_ARMOUR_DAMAGE = "ArmourDamage";
        public const string DMG_STRUCTURE_RESISTED = "ArmourResisted";
        public const string DMG_STRUCTURE_DAMAGE = "ArmourDamage";

        public const string SHOTS_FIRED = "ShotsFired";
        public const string SHOTS_RECEIVED = "ShotsReceived";
        public const string SHOTS_MISSED = "ShotsMissed";
        public const string SHOTS_DODGED = "ShotsDodged";

        public const string LOOT_LOST = "LootLost";
        public const string LOOT_WON = "LootWon";

        public const string KILLS = "Kills";
        public const string LOSSES = "Losses";

        #endregion

        #region Properties

        public int this [string key] {
            get {
                return _stats.ContainsKey (key) ? _stats [key] : 0;
            }
        }

        public IReadOnlyDictionary<string, int> Stats {
            get {
                return _stats;
            }
        }

        #endregion

        Dictionary<string, int> _stats = new Dictionary<string, int> ();

        public ShipReport () {
        }

        #region Serialization

        public ShipReport (SerializationInfo info, StreamingContext context) {
            _stats = (Dictionary<string, int>)info.GetValue ("Stats", typeof(Dictionary<string, int>));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Stats", _stats, typeof(Dictionary<string, int>));
        }

        #endregion

        public void NoteStat (string key, int change) {
            if (!_stats.ContainsKey (key)) {
                _stats [key] = 0;
            }
            _stats [key] += change;
        }
    }
}

