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
using System.Text;
using BLibrary.Resources;
using System.Text.RegularExpressions;
using System.Linq;

namespace BLibrary.Util {

    public static class StringUtils {
        public static string ToFilePath (string resource) {
            int last = resource.LastIndexOf ('.');
            string query = resource.Substring (0, last);
            string suffix = resource.Substring (last);
            return query.Replace (".", "/") + suffix;
        }

        public static string ToRomanNumeral (int number) {
            if ((number < 0) || (number > 3999))
                throw new ArgumentOutOfRangeException ("number", "Must be between 0 and 3999, was: " + number);
            if (number == 0)
                return string.Empty;
            if (number >= 1000)
                return "M" + ToRomanNumeral (number - 1000);
            if (number >= 900)
                return "CM" + ToRomanNumeral (number - 900);
            if (number >= 500)
                return "D" + ToRomanNumeral (number - 500);
            if (number >= 400)
                return "CD" + ToRomanNumeral (number - 400);
            if (number >= 100)
                return "C" + ToRomanNumeral (number - 100);
            if (number >= 90)
                return "XC" + ToRomanNumeral (number - 90);
            if (number >= 50)
                return "L" + ToRomanNumeral (number - 50);
            if (number >= 40)
                return "XL" + ToRomanNumeral (number - 40);
            if (number >= 10)
                return "X" + ToRomanNumeral (number - 10);
            if (number >= 9)
                return "IX" + ToRomanNumeral (number - 9);
            if (number >= 5)
                return "V" + ToRomanNumeral (number - 5);
            if (number >= 4)
                return "IV" + ToRomanNumeral (number - 4);
            if (number >= 1)
                return "I" + ToRomanNumeral (number - 1);

            throw new ArgumentOutOfRangeException ("Something unexpected happened while trying to parse integer to a roman numeral.");
        }

        public static string Capitalize (string input) {
            if (string.IsNullOrWhiteSpace (input))
                return input;

            char[] arr = input.ToCharArray ();
            arr [0] = Char.ToUpperInvariant (arr [0]);
            return new String (arr);
        }

        public static string Uncapitalize (string input) {
            if (string.IsNullOrWhiteSpace (input))
                return input;

            char[] arr = input.ToCharArray ();
            arr [0] = Char.ToLowerInvariant (arr [0]);
            return new String (arr);
        }

        public static string BuildListing (string delim, params string[] parts) {
            if (parts.Length <= 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder ();
            for (int i = 0; i < parts.Length; i++) {
                if (builder.Length > 0)
                    builder.Append (delim);
                builder.Append (parts [i]);
            }

            return builder.ToString ();
        }

        const string MOD_PERCENT = "{0:§\\#\\0\\0ff\\0\\0§+0%;§\\#fa8\\072§-0%;§\\#\\0\\0ee\\0\\0§0%}";

        public static string FormatModificationInfo (string unlocalized, float value) {
            return string.Format (Localization.Instance [unlocalized, string.Format (MOD_PERCENT, value)]);
        }

        /// <summary>
        /// Determines whether the given string consists only of numbers and letters.
        /// </summary>
        /// <returns><c>true</c>, if the string consisted only of numbers and letters, <c>false</c> otherwise.</returns>
        /// <param name="input">Input.</param>
        public static bool ConsistsOfOnlyNumbersAndLetters (string input) {
            return input.All (Char.IsLetterOrDigit);
        }
    }
}

