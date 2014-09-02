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
using BLibrary.Json;
using BLibrary.Serialization;
using System.Collections.Generic;
using System.Linq;
using BLibrary.Graphics;

namespace Starliners.Game.Forces {

    [Serializable]
    public sealed class ShipClass : Asset, ISpriteDeclarant {
        #region Constants

        const string NAME_PREFIX = "sclass";

        public const int MAX_TRACKING = 10000;
        public const int MAX_MANOUVER = 10000;

        #endregion

        public uint[] Icons {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public uint Cost {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public ShipSize Size {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public ShipRole Role {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Manouver {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Tracking {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int FireHeat {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int FireKinetic {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int FireRadiation {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int SupportShield {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int SupportArmour {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int SupportHull {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Armour {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Shield {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public int Hull {
            get;
            private set;
        }

        [GameData (Remote = true)]
        public Resists Resistances {
            get;
            private set;
        }

        public ISet<string> Flags {
            get {
                return _flags;
            }
        }

        public int Loot {
            get {
                switch (Size) {
                    case ShipSize.Dreadnought:
                        return 16;
                    case ShipSize.Battleship:
                        return 8;
                    case ShipSize.Cruiser:
                        return 4;
                    case ShipSize.Destroyer:
                        return 2;
                    default:
                        return 1;
                }
            }
        }

        #region Fields

        [GameData (Remote = true)]
        string _icon;
        List<Trigger> _triggers = new List<Trigger> ();
        [GameData (Remote = true)]
        List<string> _constraints = new List<string> ();
        HashSet<string> _flags = new HashSet<string> ();

        #endregion

        public ShipClass (IWorldAccess access, string name, IPopulator populator, JsonObject json)
            : base (access, Utils.BuildName (NAME_PREFIX, name), populator.KeyMap) {

            _icon = json.ContainsKey ("icon") ? json ["icon"].GetValue<string> () : "fleet4";
            Size = json.ContainsKey ("size") ? (ShipSize)Enum.Parse (typeof(ShipSize), json ["size"].GetValue<string> (), true) : ShipSize.Frigate;
            Role = json.ContainsKey ("role") ? (ShipRole)Enum.Parse (typeof(ShipRole), json ["role"].GetValue<string> (), true) : ShipRole.Combat;
            Manouver = json.ContainsKey ("manouver") ? (int)json ["manouver"].GetValue<double> () : 10;
            Tracking = json.ContainsKey ("tracking") ? (int)json ["tracking"].GetValue<double> () : 10;

            if (json.ContainsKey ("weapons")) {
                JsonObject weapons = json ["weapons"].GetValue<JsonObject> ();
                FireHeat = weapons.ContainsKey ("heat") ? (int)weapons ["heat"].GetValue<double> () : 0;
                FireKinetic = weapons.ContainsKey ("kinetic") ? (int)weapons ["kinetic"].GetValue<double> () : 0;
                FireRadiation = weapons.ContainsKey ("radioactive") ? (int)weapons ["radioactive"].GetValue<double> () : 0;
            }

            if (json.ContainsKey ("structure")) {
                JsonObject structure = json ["structure"].GetValue<JsonObject> ();
                Armour = structure.ContainsKey ("armour") ? (int)structure ["armour"].GetValue<double> () : 100;
                Shield = structure.ContainsKey ("shield") ? (int)structure ["shield"].GetValue<double> () : 100;
                Hull = structure.ContainsKey ("hull") ? (int)structure ["hull"].GetValue<double> () : 100;
            }

            if (json.ContainsKey ("support")) {
                JsonObject support = json ["support"].GetValue<JsonObject> ();
                SupportShield = support.ContainsKey ("shield") ? (int)support ["shield"].GetValue<double> () : 0;
                SupportArmour = support.ContainsKey ("armour") ? (int)support ["armour"].GetValue<double> () : 0;
                SupportHull = support.ContainsKey ("hull") ? (int)support ["hull"].GetValue<double> () : 0;
            }

            Cost = (uint)(FireHeat + FireKinetic + FireRadiation + Armour + Shield + Hull);
            Resistances = json.ContainsKey ("resists") ? new Resists (json ["resists"].GetValue<JsonObject> ()) : new Resists (1.0f, 1.0f, 1.0f);

            if (json.ContainsKey ("constraints")) {
                foreach (string str in json["constraints"].AsEnumerable<string>()) {
                    _constraints.Add (str);
                }
            }
            if (json.ContainsKey ("flags")) {
                foreach (string str in json["flags"].AsEnumerable<string>()) {
                    _flags.Add (str);
                }
            }

            if (json.ContainsKey ("trigger")) {
                foreach (JsonObject obj in json["trigger"].AsEnumerable<JsonObject>()) {
                    _triggers.Add (Trigger.InstantiateTrigger (access, populator, obj));
                }
            }
        }

        #region Serialization

        public ShipClass (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Indicates whether budget needs to be allocated for this ship class on the given planet.
        /// </summary>
        /// <returns><c>true</c>, if budget was needsed, <c>false</c> otherwise.</returns>
        /// <param name="planet">Planet.</param>
        public bool NeedsBudget (ILevyProvider planet) {
            // No trigger means this is spawned only.
            if (_triggers.Count <= 0) {
                return false;
            }
            for (int i = 0; i < _triggers.Count; i++) {
                if (!_triggers [i].IsTripped (planet)) {
                    return false;
                }
            }
            return true;
        }

        public int DetermineMaxLevy (ILevyProvider planet) {
            int max = planet.GetMaintenance (Size);
            for (int i = 0; i < _constraints.Count; i++) {
                int attr = planet.GetAttribute (_constraints [i]);
                max = max > attr ? attr : max;
            }
            return max;
        }

        public float DetermineLevyGrowth (ILevyProvider planet) {
            return (float)planet.Reenforcement / Cost;
        }

        public static IEnumerable<ShipClass> GetClassesForWorld (IWorldAccess access) {
            return access.Assets.Values.OfType<ShipClass> ();
        }

        public void RegisterIcons (IIconRegister register) {
            Icons = register.RegisterIcon (_icon);
        }

        public override string ToString () {
            return string.Format ("[ShipClass: Serial={0}, Name={1}, Size={2}, Manouver={3}]", Serial, Name, Size, Manouver);
        }
    }
}

