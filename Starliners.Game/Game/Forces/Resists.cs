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

namespace Starliners.Game.Forces {

    [Serializable]
    public sealed class Resists : ISerializable {

        public readonly float Heat;
        public readonly float Kinetic;
        public readonly float Radiation;

        public Resists (float heat, float kinetic, float radiation) {
            Heat = heat;
            Kinetic = kinetic;
            Radiation = radiation;
        }

        public Resists (JsonObject json) {
            Heat = json.ContainsKey ("heat") ? (float)json ["heat"].GetValue<double> () : 0.1f;
            Kinetic = json.ContainsKey ("kinetic") ? (float)json ["kinetic"].GetValue<double> () : 0.1f;
            Radiation = json.ContainsKey ("radiation") ? (float)json ["radiation"].GetValue<double> () : 0.1f;
        }

        #region Serialization

        public Resists (SerializationInfo info, StreamingContext context) {
            Heat = info.GetSingle ("Heat");
            Kinetic = info.GetSingle ("Kinetic");
            Radiation = info.GetSingle ("Radiation");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Heat", Heat);
            info.AddValue ("Kinetic", Kinetic);
            info.AddValue ("Radiation", Radiation);
        }

        #endregion

        public int AbsorbDamage (DamageKind kind, int damage) {
            switch (kind) {
                case DamageKind.Heat:
                    return (int)(damage * Heat);
                case DamageKind.Kinetic:
                    return (int)(damage * Kinetic);
                case DamageKind.Radiation:
                    return (int)(damage * Radiation);
                default:
                    return 0;
            }
        }

        public override string ToString () {
            return string.Format ("[Resists: Heat={0}, Kinetic={1}, Radiation={2}]", Heat, Kinetic, Radiation);
        }

        public static Resists Average (Resists resists, Resists other) {
            return new Resists (
                (resists.Heat + other.Heat) / 2,
                (resists.Kinetic + other.Kinetic) / 2,
                (resists.Radiation + other.Radiation) / 2
            );
        }

        public static Resists Adjust (Resists resists, Resists other) {
            return new Resists (
                resists.Heat * other.Heat,
                resists.Kinetic * other.Kinetic,
                resists.Radiation * other.Radiation
            );
        }

        public static Resists Adjust (Resists resists, DamageKind kind, float adjustment) {
            float heat = resists.Heat * (kind == DamageKind.Heat ? adjustment : 1.0f); 
            float kinetic = resists.Kinetic * (kind == DamageKind.Kinetic ? adjustment : 1.0f); 
            float radiation = resists.Radiation * (kind == DamageKind.Radiation ? adjustment : 1.0f);
            return new Resists (heat, kinetic, radiation);
        }

        public static Resists Adjust (Resists resists, ShipModifiers modifiers) {
            float heat = resists.Heat * (float)(1 + modifiers.ModResistHeat); 
            float kinetic = resists.Kinetic * (float)(1 + modifiers.ModResistKinetic); 
            float radiation = resists.Radiation * (float)(1 + modifiers.ModResistRadiation);
            return new Resists (heat, kinetic, radiation);
        }

        public static Resists EnforceMax (Resists resists, float limit) {
            return new Resists (
                resists.Heat > limit ? limit : resists.Heat,
                resists.Kinetic > limit ? limit : resists.Kinetic,
                resists.Radiation > limit ? limit : resists.Radiation
            );
        }
    }
}

