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
using BLibrary.Util;

namespace Starliners.Game.Forces {
    [Serializable]
    public struct DamageReport : ISerializable {

        public static readonly DamageReport NO_DAMAGE = new DamageReport (0, 0, 0, 0, 0, 0, HitType.Missed);

        #region Properties

        public bool NoEffect {
            get {
                return ShieldResisted <= 0 && ShieldDamage <= 0
                && ArmourResisted <= 0 && ArmourDamage <= 0
                && StructureResisted <= 0 && StructureDamage <= 0;
            }
        }

        public Penetration Penetrated {
            get {
                if (StructureDamage > 0) {
                    return Penetration.Hull;
                }
                if (ArmourDamage > 0) {
                    return Penetration.Armour;
                }
                return Penetration.Shield;
            }
        }

        public int Delivered {
            get {
                return ShieldDamage + ArmourDamage + StructureDamage;
            }
        }

        public int Resisted {
            get {
                return ShieldResisted + ArmourResisted + StructureResisted;
            }
        }

        #endregion

        public readonly int ShieldResisted;
        public readonly int ShieldDamage;

        public readonly int ArmourResisted;
        public readonly int ArmourDamage;

        public readonly int StructureResisted;
        public readonly int StructureDamage;

        public readonly HitType Type;

        public DamageReport (int ashield, int dshield, int aarmour, int darmour, int astructure, int dstructure, HitType type) {
            ShieldResisted = ashield;
            ShieldDamage = dshield;

            ArmourResisted = aarmour;
            ArmourDamage = darmour;

            StructureResisted = astructure;
            StructureDamage = dstructure;

            Type = type;
        }

        public DamageReport (SerializationInfo info, StreamingContext context) {
            ShieldResisted = info.GetInt32 ("SAbs");
            ShieldDamage = info.GetInt32 ("SDam");
            ArmourResisted = info.GetInt32 ("AAbs");
            ArmourDamage = info.GetInt32 ("ADam");
            StructureResisted = info.GetInt32 ("HAbs");
            StructureDamage = info.GetInt32 ("HDam");
            Type = (HitType)info.GetInt32 ("HType");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("SAbs", ShieldResisted);
            info.AddValue ("SDam", ShieldDamage);
            info.AddValue ("AAbs", ArmourResisted);
            info.AddValue ("ADam", ArmourDamage);
            info.AddValue ("HAbs", StructureResisted);
            info.AddValue ("HDam", StructureDamage);
            info.AddValue ("HType", (int)Type);
        }

        public IEnumerable<string> GetInfo () {
            IList<string> info = new List<string> ();

            string formatting = string.Empty;
            if (Type.HasFlag (HitType.Critical) || Type.HasFlag (HitType.Final)) {
                formatting = "?+b";
            }
            if (ShieldResisted > 0 || ShieldDamage > 0) {
                info.Add (FormatDamage (Colour.Turquoise, formatting, ShieldDamage, ShieldResisted));
            }
            if (ArmourResisted > 0 || ArmourDamage > 0) {
                info.Add (FormatDamage (Colour.Yellow, formatting, ArmourDamage, ArmourResisted));
            }
            if (StructureResisted > 0 || StructureDamage > 0) {
                info.Add (FormatDamage (Colour.Red, formatting, StructureDamage, StructureResisted));
            }

            return info;
        }

        string FormatDamage (Colour colour, string formatting, int damage, int absorbed) {
            return string.Format ("§{0}{1}§{2}{3}", colour.ToString ("#"), formatting, damage.ToString (), absorbed > 0 ? string.Format ("({0})", absorbed) : string.Empty);
        }

        public override string ToString () {
            return string.Format ("[DamageReport: SAbs={0}, SDam={1}, AAbs={2}, ADam={3}, HAbs={4}, HDam={5}, Type={6}]", ShieldResisted, ShieldDamage, ArmourResisted, ArmourDamage, StructureResisted, StructureDamage, Type.ToString ());
        }
    }
}

