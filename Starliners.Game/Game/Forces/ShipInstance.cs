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

namespace Starliners.Game.Forces {
    /// <summary>
    /// Represents a ship engaged in battle.
    /// </summary>
    [Serializable]
    public sealed class ShipInstance : SerializableObject {

        public ShipState State {
            get {
                if (Construction < 1) {
                    return ShipState.UnderConstruction;
                }
                if (_layerHull <= 0) {
                    return ShipState.Wreck;
                }
                if (_layerArmour <= 0) {
                    return ShipState.HullDamage;
                }
                if (_layerShield <= 0) {
                    return ShipState.ArmourDamage;
                }

                return ShipState.Shielded;
            }
        }

        [GameData (Remote = true, Key = "Inception")]
        public long Inception {
            get;
            private set;
        }

        /// <summary>
        /// Indicates when the ship last joined a battle.
        /// </summary>
        /// <value>The last joined.</value>
        [GameData (Remote = true, Key = "LastJoined")]
        public long LastJoined {
            get;
            set;
        }

        [GameData (Remote = true, Key = "ShipClass")]
        public ShipClass ShipClass {
            get;
            private set;
        }

        public ILevyProvider Origin {
            get {
                return _levy.Origin;
            }
        }

        public ShipProperties Properties {
            get {
                return _properties;
            }
        }

        public ColourScheme Colours {
            get {
                return Origin.Owner.Colours;
            }
        }

        public ShipLevel Level {
            get { 
                double actual = Math.Sqrt (Experience) / 40;
                if (actual >= 1) {
                    return ShipLevel.Elite;
                }
                return (ShipLevel)(1 + (4 * actual));
            }
        }

        public float Health {
            get {
                float max = ShipClass.Shield + ShipClass.Armour + ShipClass.Hull;
                float actual = _layerShield + _layerArmour + _layerHull;
                return actual / max;
            }
        }

        [GameData (Remote = true, Key = "Experience")]
        public int Experience {
            get;
            private set;
        }

        public int LayerShield {
            get {
                return _layerShield;
            }
        }

        public int LayerArmour {
            get {
                return _layerArmour;
            }
        }

        public int LayerHull {
            get {
                return _layerHull;
            }
        }

        [GameData (Remote = true, Key = "Construction")]
        public float Construction {
            get;
            private set;
        }

        public ShipProjector Projector {
            get {
                _projector.State = State;
                return _projector;
            }
        }

        #region Fields

        [GameData (Remote = true, Key = "Levy")]
        readonly Levy _levy;

        [GameData (Remote = true, Key = "Properties")]
        ShipProperties _properties;

        [GameData (Remote = true, Key = "LayerShield")]
        int _layerShield;
        [GameData (Remote = true, Key = "LayerArmour")]
        int _layerArmour;
        [GameData (Remote = true, Key = "LayerHull")]
        int _layerHull;

        ShipProjector _projector;

        #endregion

        public ShipInstance (IWorldAccess access, Levy levy, ShipClass sclass, ShipModifiers modifiers)
            : base (access) {

            Inception = access.Clock.Ticks;

            _levy = levy;
            ShipClass = sclass;

            _layerShield = ShipClass.Shield;
            _layerArmour = ShipClass.Armour;
            _layerHull = ShipClass.Hull;

            ResetProperties (modifiers);
        }

        #region Serialization

        public ShipInstance (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        protected override void OnCommissioned () {
            base.OnCommissioned ();

            _projector = new ShipProjector (ShipClass.Icons, Colours.Vessels, Colours.Shields, Serial.GetHashCode ());
        }

        #endregion

        public void ResetProperties (ShipModifiers modifiers) {
            _properties = new ShipProperties (ShipClass, modifiers, _levy.Origin.Owner.CombatProperties);
        }

        public float ApplyConstruction (float construction) {
            Construction += construction;

            float unused = 0;
            if (Construction > 1) {
                unused = Construction - 1;
                Construction = 1;
            }
            return unused;
        }

        public DamageReport AbsorbVolley (Volley volley) {

            // Determine type of hit by rolling evasion and tracking for each hit level.
            double evasion = (double)ShipLevels.GetManouver (Level, ShipClass.Manouver) / ShipClass.MAX_MANOUVER;
            double tracking = (double)volley.Tracking / ShipClass.MAX_TRACKING;

            HitType type = HitType.None;
            for (int i = 0; i < HitTypes.VALID_HITS.Length; i++) {
                // Skip to the next hit level if we failed evasion.
                if (Access.Rand.NextDouble () >= evasion) {
                    continue;
                }
                // Skip to the next hit level if tracking overcame evasion anyway.
                if (Access.Rand.NextDouble () < tracking) {
                    continue;
                }
                // Ship managed to evade and tracking did not overcome evasion,
                // so this is our max hit level.
                type = HitTypes.VALID_HITS [i];
                break;
            }

            if (type == HitType.Missed) {
                Access.GameConsole.Log (Battle.LOG_LEVEL, "MISSED\n\t\t{0}\n\t\t{1}\n\t\tEvasion: {2} <-> Tracking: {3}.", volley, this, evasion, tracking);
                return DamageReport.NO_DAMAGE;
            }

            // Evasion failed, the volley hit
            int damage = HitTypes.GetRandomizedDamage (Access.Rand, type, volley.Damage);

            int cshield = 0;
            int rshield = 0;
            ApplyDamage (_layerShield, volley.Kind, damage, _properties.ResistsShield, out cshield, out rshield);
            _layerShield -= rshield;
            damage -= cshield;

            int carmour = 0;
            int rarmour = 0;
            ApplyDamage (_layerArmour, volley.Kind, damage, _properties.ResistsArmour, out carmour, out rarmour);
            _layerArmour -= rarmour;
            damage -= carmour;

            int cstructure = 0;
            int rstructure = 0;
            ApplyDamage (_layerHull, volley.Kind, damage, _properties.ResistsHull, out cstructure, out rstructure);
            _layerHull -= rstructure;
            damage -= cstructure;

            type = State == ShipState.Wreck ? type | HitType.Final : type;
            return new DamageReport (cshield - rshield, rshield, carmour - rarmour, rarmour, cstructure - rstructure, rstructure, type);
        }

        public Volley Fire (DamageKind kind) {
            int damage = 0;
            switch (kind) {
                case DamageKind.Heat:
                    damage = _properties.FireHeat;
                    break;
                case DamageKind.Kinetic:
                    damage = _properties.FireKinetic;
                    break;
                case DamageKind.Radiation:
                    damage = _properties.FireRadiation;
                    break;
            }
            return new Volley (kind, ShipLevels.GetTracking (Level, ShipClass.Tracking), damage);
        }

        public int Support (StructureLayer layer) {
            switch (layer) {
                case StructureLayer.Shield:
                    return _properties.SupportShield;
                case StructureLayer.Armour:
                    return _properties.SupportArmour;
                case StructureLayer.Hull:
                    return _properties.SupportHull;
            }
            return 0;
        }

        public int RequiresHealing (StructureLayer layer) {
            switch (layer) {
                case StructureLayer.Shield:
                    return ShipClass.Shield - _layerShield;
                case StructureLayer.Armour:
                    return ShipClass.Armour - _layerArmour;
                case StructureLayer.Hull:
                    return ShipClass.Hull - _layerHull;
            }
            return 0;
        }

        public void ApplyHealing (Regen regen) {
            switch (regen.Layer) {
                case StructureLayer.Shield:
                    _layerShield += regen.Healed;
                    break;
                case StructureLayer.Armour:
                    _layerArmour += regen.Healed;
                    break;
                case StructureLayer.Hull:
                    _layerHull += regen.Healed;
                    break;
            }
        }

        void ApplyDamage (int layer, DamageKind kind, int damage, Resists resistances, out int consumed, out int reduction) {
            if (layer <= 0 || damage <= 0) {
                consumed = 0;
                reduction = 0;
                return;
            }

            int absorbed = resistances.AbsorbDamage (kind, damage);
            //absorbed = absorbed > layer ? layer : absorbed;
            damage -= absorbed;
            int hit = damage > layer ? layer : damage;
            consumed = absorbed + hit;
            reduction = hit;
        }

        public void GainExperience (int experience) {
            Experience += experience;
        }

        /// <summary>
        /// Turns the ship into a wrecked hulk.
        /// </summary>
        public void Destroy () {
            _layerShield = 0;
            _layerArmour = 0;
            _layerHull = 0;
        }

        public void OnRemovedFromBattle (bool wasDestroyed) {
            if (wasDestroyed) {
                _levy.OnShipLoss (this);
            }
        }

        public override string ToString () {
            return string.Format ("[ShipInstance: Serial={0}, ShipClass={1}]", Serial, ShipClass);
        }
    }
}