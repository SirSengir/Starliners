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
using BLibrary.Resources;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class ShipModifiers : ISerializable, IEquatable<ShipModifiers>, IDescribable {
        #region Constants

        static readonly List<string> NO_INFO = new List<string> ();

        #endregion

        #region Properties

        public string Description {
            get {
                return Localization.Instance ["tt_ship_modifications"];
            }
        }

        public double ModFireHeat {
            get {
                return Math.Sqrt (FocusHeat) / 100;
            }
        }

        public double ModFireKinetic {
            get {
                return Math.Sqrt (FocusKinetic) / 100;
            }
        }

        public double ModFireRadiation {
            get {
                return Math.Sqrt (FocusRadiation) / 100;
            }
        }

        public double ModResistHeat {
            get {
                return Math.Sqrt (ResistHeat) / 100;
            }
        }

        public double ModResistKinetic {
            get {
                return Math.Sqrt (ResistKinetic) / 100;
            }
        }

        public double ModResistRadiation {
            get {
                return Math.Sqrt (ResistRadiation) / 100;
            }
        }

        #endregion

        #region Fields

        public readonly int ResistHeat;
        public readonly int ResistKinetic;
        public readonly int ResistRadiation;

        public readonly int FocusHeat;
        public readonly int FocusKinetic;
        public readonly int FocusRadiation;

        #endregion

        public ShipModifiers (int rheat, int rkinetic, int rradiation, int fheat, int fkinetic, int fradiation) {

            ResistHeat = rheat;
            ResistKinetic = rkinetic;
            ResistRadiation = rradiation;

            FocusHeat = fheat;
            FocusKinetic = fkinetic;
            FocusRadiation = fradiation;
        }

        public ShipModifiers () {
        }

        #region Serialization

        public ShipModifiers (SerializationInfo info, StreamingContext context) {
            ResistHeat = info.GetInt32 ("RHeat");
            ResistKinetic = info.GetInt32 ("RKinetic");
            ResistRadiation = info.GetInt32 ("RRadiation");

            FocusHeat = info.GetInt32 ("FHeat");
            FocusKinetic = info.GetInt32 ("FKinetic");
            FocusRadiation = info.GetInt32 ("FRadiation");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("RHeat", ResistHeat);
            info.AddValue ("RKinetic", ResistKinetic);
            info.AddValue ("RRadiation", ResistRadiation);

            info.AddValue ("FHeat", FocusHeat);
            info.AddValue ("FKinetic", FocusKinetic);
            info.AddValue ("FRadiation", FocusRadiation);
        }

        #endregion

        public IList<string> GetInformation (Player player) {
            List<string> infos = new List<string> ();
            if (FocusHeat > 0) {
                infos.Add (string.Format ("Heat Damage: {0:P1}", ModFireHeat));
            }
            if (FocusKinetic > 0) {
                infos.Add (string.Format ("Kinetic Damage: {0:P1}", ModFireKinetic));
            }
            if (FocusRadiation > 0) {
                infos.Add (string.Format ("Radiation Damage: {0:P1}", ModFireRadiation));
            }

            if (infos.Count <= 0) {
                infos.Add (Localization.Instance ["tt_ship_modifications_none"]);
            }
            return infos;
        }

        public IList<string> GetUsage (Player player) {
            return NO_INFO;
        }

        public override int GetHashCode () {
            return ResistHeat ^ ResistKinetic ^ ResistRadiation ^ FocusHeat ^ FocusKinetic ^ FocusRadiation;
        }

        public override bool Equals (object obj) {
            ShipModifiers modifiers = obj as ShipModifiers;
            if (modifiers == null) {
                return false;
            }

            return Equals (modifiers);
        }

        public bool Equals (ShipModifiers other) {
            if (ResistHeat != other.ResistHeat) {
                return false;
            }
            if (ResistKinetic != other.ResistKinetic) {
                return false;
            }
            if (ResistRadiation != other.ResistRadiation) {
                return false;
            }
            if (FocusHeat != other.FocusHeat) {
                return false;
            }
            if (FocusKinetic != other.FocusKinetic) {
                return false;
            }
            if (FocusRadiation != other.FocusRadiation) {
                return false;
            }
            return true;
        }

    }
}

