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
using System.Linq;

namespace Starliners.Game {
    [Serializable]
    public sealed class CombatProperties : ISerializable {

        public readonly DamageKind ResistWeakness;
        public readonly DamageKind ResistStrength;

        public readonly DamageKind WeaponWeakness;
        public readonly DamageKind WeaponStrength;

        public CombatProperties (Random rand) {
            List<DamageKind> kinds = DamageKinds.VALID_VALUES.ToList ();
            ResistWeakness = kinds [rand.Next (kinds.Count)];
            kinds.Remove (ResistWeakness);
            ResistStrength = kinds [rand.Next (kinds.Count)];

            kinds = DamageKinds.VALID_VALUES.ToList ();
            WeaponWeakness = kinds [rand.Next (kinds.Count)];
            kinds.Remove (WeaponWeakness);
            WeaponStrength = kinds [rand.Next (kinds.Count)];
        }

        #region Serialization

        public CombatProperties (SerializationInfo info, StreamingContext context) {
            ResistWeakness = (DamageKind)info.GetInt32 ("RWeakness");
            ResistStrength = (DamageKind)info.GetInt32 ("RStrength");
            WeaponWeakness = (DamageKind)info.GetInt32 ("WWeakness");
            WeaponStrength = (DamageKind)info.GetInt32 ("WStrength");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("RWeakness", (int)ResistWeakness);
            info.AddValue ("RStrength", (int)ResistStrength);
            info.AddValue ("WWeakness", (int)WeaponWeakness);
            info.AddValue ("WStrength", (int)WeaponStrength);
        }

        #endregion

        public override string ToString () {
            return string.Format ("[CombatProperties: ResistWeakness={0}, ResistStrength={1}, WeaponWeakness={2}, WeaponStrength={3}]",
                ResistWeakness, ResistStrength, WeaponWeakness, WeaponStrength);
        }
    }
}

