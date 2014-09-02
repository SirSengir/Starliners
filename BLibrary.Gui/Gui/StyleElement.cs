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
using BLibrary.Util;
using Starliners;

namespace BLibrary.Gui {

    public class StyleElement {

        #region Properties

        public Tinting Tinting {
            get;
            private set;
        }

        #endregion

        public StyleElement (IInterfaceDefinition provider, JsonObject json) {
            if (json.ContainsKey ("tinting")) {
                JsonObject tinter = json ["tinting"].GetValue<JsonObject> ();
                string tint = tinter ["type"].GetValue<string> ();
                Colour colour = tinter.ContainsKey ("colour") ? new Colour (Int32.Parse (tinter ["colour"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : Colour.White;
                Tinting = CreateTinting (tint, colour);
            } else {
                Tinting = NoTinting.INSTANCE;
            }
        }

        private Tinting CreateTinting (string tint, Colour colour) {
            switch (tint) {
                case "parent":
                    return new ParentTinting ();
                case "background":
                    return new BackgroundTinting ();
                case "static":
                    return new StaticTinting (colour);
                case "none":
                default:
                    return NoTinting.INSTANCE;
            }
        }

    }
}

