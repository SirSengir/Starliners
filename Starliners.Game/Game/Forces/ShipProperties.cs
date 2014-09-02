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

namespace Starliners.Game.Forces {
    /// <summary>
    /// These are an individual ship's attributes made concrete.
    /// </summary>
    /// <remarks>Takes ship class and modifications by squadron into account.</remarks>
    [Serializable]
    public sealed class ShipProperties : ISerializable {
        #region Constants

        static readonly Resists RESISTS_SHIELD = new Resists (0.3f, 0.3f, 0.1f);
        static readonly Resists RESISTS_ARMOUR = new Resists (0.3f, 0.1f, 0.3f);
        static readonly Resists RESISTS_HULL = new Resists (0.1f, 0.3f, 0.3f);

        #endregion

        public int FullBarrage {
            get {
                return FireHeat + FireKinetic + FireRadiation;
            }
        }

        public readonly Resists ResistsShield;
        public readonly Resists ResistsArmour;
        public readonly Resists ResistsHull;

        public readonly int FireRadiation;
        public readonly int FireHeat;
        public readonly int FireKinetic;

        public readonly int SupportShield;
        public readonly int SupportArmour;
        public readonly int SupportHull;

        public ShipProperties (ShipClass sclass, ShipModifiers modifiers, CombatProperties properties) {
            ResistsShield = Resists.Adjust (RESISTS_SHIELD, sclass.Resistances);
            ResistsArmour = Resists.Adjust (RESISTS_ARMOUR, sclass.Resistances);
            ResistsHull = Resists.Adjust (RESISTS_HULL, sclass.Resistances);

            ResistsShield = Resists.Adjust (ResistsShield, properties.ResistWeakness, 0.8f);
            ResistsShield = Resists.Adjust (ResistsShield, properties.ResistStrength, 1.2f);
            ResistsShield = Resists.Adjust (ResistsShield, modifiers);
            ResistsShield = Resists.EnforceMax (ResistsShield, 0.95f);

            ResistsArmour = Resists.Adjust (ResistsArmour, properties.ResistWeakness, 0.8f);
            ResistsArmour = Resists.Adjust (ResistsArmour, properties.ResistStrength, 1.2f);
            ResistsArmour = Resists.Adjust (ResistsArmour, modifiers);
            ResistsArmour = Resists.EnforceMax (ResistsArmour, 0.95f);

            ResistsHull = Resists.Adjust (ResistsHull, properties.ResistWeakness, 0.8f);
            ResistsHull = Resists.Adjust (ResistsHull, properties.ResistStrength, 1.2f);
            ResistsHull = Resists.Adjust (ResistsHull, modifiers);
            ResistsHull = Resists.EnforceMax (ResistsHull, 0.95f);

            FireHeat = (int)(sclass.FireHeat * GetAdjustment (DamageKind.Heat, properties.WeaponStrength, properties.WeaponWeakness) * (1 + modifiers.ModFireHeat));
            FireKinetic = (int)(sclass.FireKinetic * GetAdjustment (DamageKind.Kinetic, properties.WeaponStrength, properties.WeaponWeakness) * (1 + modifiers.ModFireKinetic));
            FireRadiation = (int)(sclass.FireRadiation * GetAdjustment (DamageKind.Radiation, properties.WeaponStrength, properties.WeaponWeakness) * (1 + modifiers.ModFireRadiation));

            SupportShield = sclass.SupportShield;
            SupportArmour = sclass.SupportArmour;
            SupportHull = sclass.SupportHull;
        }

        #region Serialization

        public ShipProperties (SerializationInfo info, StreamingContext context) {
            ResistsShield = (Resists)info.GetValue ("RShields", typeof(Resists));
            ResistsArmour = (Resists)info.GetValue ("RArmour", typeof(Resists));
            ResistsHull = (Resists)info.GetValue ("RHull", typeof(Resists));
            FireRadiation = info.GetInt32 ("FireRadiation");
            FireHeat = info.GetInt32 ("FireHeat");
            FireKinetic = info.GetInt32 ("FireKinetic");
            SupportShield = info.GetInt32 ("SShield");
            SupportArmour = info.GetInt32 ("SArmour");
            SupportHull = info.GetInt32 ("SHull");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("RShields", ResistsShield, typeof(Resists));
            info.AddValue ("RArmour", ResistsArmour, typeof(Resists));
            info.AddValue ("RHull", ResistsHull, typeof(Resists));
            info.AddValue ("FireRadiation", FireRadiation);
            info.AddValue ("FireHeat", FireHeat);
            info.AddValue ("FireKinetic", FireKinetic);
            info.AddValue ("SShield", SupportShield);
            info.AddValue ("SArmour", SupportArmour);
            info.AddValue ("SHull", SupportHull);
        }

        #endregion

        double GetAdjustment (DamageKind toadjust, DamageKind strength, DamageKind weakness) {
            if (toadjust == strength) {
                return 1.2;
            }
            return toadjust == weakness ? 0.8 : 1.0;
        }

    }
}

