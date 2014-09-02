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

namespace BLibrary.Util {

    public class MoneyFormatter : IFormatProvider, ICustomFormatter {
        static MoneyFormatter _instance;

        public static MoneyFormatter Instance {
            get {
                if (_instance == null) {
                    _instance = new MoneyFormatter ();
                }
                return _instance;
            }
        }

        public string Format (string format, object arg, IFormatProvider formatProvider) {
            return FormatVolume ((decimal)arg);
        }

        public object GetFormat (Type formatType) {
            return this;
        }

        string FormatVolume (decimal volume) {
            if (volume >= 900000000) {
                return string.Format ("{0:0.#}B", (volume / 1000000000));
            } else if (volume >= 900000) {
                return string.Format ("{0:0.#}M", (volume / 1000000));
            } else if (volume >= 9000) {
                return string.Format ("{0:0.#}K", (volume / 1000));
            } else {
                return string.Format ("{0:0.#}", volume);
            }
        }
    }
}

