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

namespace Starliners.Game {
    [Serializable]
    public sealed class ColourScheme : ISerializable {

        public Colour Empire {
            get;
            private set;
        }

        public Colour Vessels {
            get;
            private set;
        }

        public Colour Shields {
            get;
            private set;
        }

        public Colour Weapons {
            get;
            private set;
        }

        public ColourScheme (Colour empire, Colour vessels, Colour shields, Colour weapons) {
            Empire = empire;
            Vessels = vessels;
            Shields = shields;
            Weapons = weapons;
        }

        public ColourScheme (JsonObject json) {
            Empire = json.ContainsKey ("empire") ? new Colour (int.Parse (json ["empire"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : Colour.DarkGray;
            Vessels = json.ContainsKey ("hulks") ? new Colour (int.Parse (json ["hulks"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : Colour.DarkGray;
            Shields = json.ContainsKey ("shields") ? new Colour (int.Parse (json ["shields"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : Colour.Turquoise;
            Weapons = json.ContainsKey ("weapons") ? new Colour (int.Parse (json ["weapons"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : Colour.LightPink;
        }

        #region Serialization

        public ColourScheme (SerializationInfo info, StreamingContext context) {
            Empire = new Colour (info.GetInt32 ("Empire"));
            Vessels = new Colour (info.GetInt32 ("Vessels"));
            Shields = new Colour (info.GetInt32 ("Shields"));
            Weapons = new Colour (info.GetInt32 ("Weapons"));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Empire", Empire.ToInteger ());
            info.AddValue ("Vessels", Vessels.ToInteger ());
            info.AddValue ("Shields", Shields.ToInteger ());
            info.AddValue ("Weapons", Weapons.ToInteger ());
        }

        #endregion

    }
}

