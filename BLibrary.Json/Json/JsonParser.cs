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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BLibrary.Json {

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// 
    /// MIT License, see http://techblog.procurios.nl/k/news/view/14605/14863/how-do-i-write-my-own-parser-(for-json).html
    /// </summary>
    public sealed class JsonParser {
        public const int TOKEN_NONE = 0;
        public const int TOKEN_CURLY_OPEN = 1;
        public const int TOKEN_CURLY_CLOSE = 2;
        public const int TOKEN_SQUARED_OPEN = 3;
        public const int TOKEN_SQUARED_CLOSE = 4;
        public const int TOKEN_COLON = 5;
        public const int TOKEN_COMMA = 6;
        public const int TOKEN_STRING = 7;
        public const int TOKEN_NUMBER = 8;
        public const int TOKEN_TRUE = 9;
        public const int TOKEN_FALSE = 10;
        public const int TOKEN_NULL = 11;

        const int BUILDER_CAPACITY = 2000;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static JsonNode JsonDecode (string json) {
            bool success = true;

            return JsonDecode (json, ref success);
        }

        /// <summary>
        /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="success">Successful parse?</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static JsonNode JsonDecode (string json, ref bool success) {
            success = true;
            if (json != null) {
                char[] charArray = json.ToCharArray ();
                int index = 0;
                JsonNode value = ParseValue (charArray, ref index, ref success);
                return value;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Converts a Hashtable / ArrayList object into a JSON string
        /// </summary>
        /// <param name="json">A Hashtable / ArrayList</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string JsonEncode (object json) {
            StringBuilder builder = new StringBuilder (BUILDER_CAPACITY);
            bool success = SerializeValue (json, builder);
            return (success ? builder.ToString () : null);
        }

        static JsonObject ParseObject (char[] json, ref int index, ref bool success) {
            Dictionary<string, JsonNode> table = new Dictionary<string, JsonNode> ();
            int token;

            // {
            NextToken (json, ref index);

            bool done = false;
            while (!done) {
                token = LookAhead (json, index);
                if (token == JsonParser.TOKEN_NONE) {
                    success = false;
                    return null;
                } else if (token == JsonParser.TOKEN_COMMA) {
                    NextToken (json, ref index);
                } else if (token == JsonParser.TOKEN_CURLY_CLOSE) {
                    NextToken (json, ref index);
                    return new JsonObject (table);
                } else {

                    // name
                    string name = ParseString (json, ref index, ref success);
                    if (!success) {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken (json, ref index);
                    if (token != JsonParser.TOKEN_COLON) {
                        success = false;
                        return null;
                    }

                    // value
                    JsonNode value = ParseValue (json, ref index, ref success);
                    if (!success) {
                        success = false;
                        return null;
                    }

                    table [name] = value;
                }
            }

            return new JsonObject (table);
        }

        static JsonArray ParseArray (char[] json, ref int index, ref bool success) {
            List<JsonNode> array = new List<JsonNode> ();

            // [
            NextToken (json, ref index);

            bool done = false;
            while (!done) {
                int token = LookAhead (json, index);
                if (token == JsonParser.TOKEN_NONE) {
                    success = false;
                    return null;
                } else if (token == JsonParser.TOKEN_COMMA) {
                    NextToken (json, ref index);
                } else if (token == JsonParser.TOKEN_SQUARED_CLOSE) {
                    NextToken (json, ref index);
                    break;
                } else {
                    JsonNode value = ParseValue (json, ref index, ref success);
                    if (!success) {
                        return null;
                    }

                    array.Add (value);
                }
            }

            return new JsonArray (array);
        }

        static JsonNode ParseValue (char[] json, ref int index, ref bool success) {
            switch (LookAhead (json, index)) {
                case JsonParser.TOKEN_STRING:
                    return new JsonNode<string> (JsonValueType.String, ParseString (json, ref index, ref success));
                case JsonParser.TOKEN_NUMBER:
                    return new JsonNode<double> (JsonValueType.Number, ParseNumber (json, ref index, ref success));
                case JsonParser.TOKEN_CURLY_OPEN:
                    return new JsonNode<JsonObject> (JsonValueType.Object, ParseObject (json, ref index, ref success));
                case JsonParser.TOKEN_SQUARED_OPEN:
                    return new JsonNode<JsonArray> (JsonValueType.Array, ParseArray (json, ref index, ref success));
                case JsonParser.TOKEN_TRUE:
                    NextToken (json, ref index);
                    return new JsonNode<bool> (JsonValueType.Boolean, true);
                case JsonParser.TOKEN_FALSE:
                    NextToken (json, ref index);
                    return new JsonNode<bool> (JsonValueType.Boolean, false);
                case JsonParser.TOKEN_NULL:
                    NextToken (json, ref index);
                    return new JsonNode<object> (JsonValueType.Null, null);
                case JsonParser.TOKEN_NONE:
                    return new JsonNode<object> (JsonValueType.None, null);
            }

            success = false;
            return null;
        }

        static string ParseString (char[] json, ref int index, ref bool success) {
            StringBuilder s = new StringBuilder (BUILDER_CAPACITY);
            char c;

            EatWhitespace (json, ref index);

            // "
            c = json [index++];

            bool complete = false;
            while (!complete) {

                if (index == json.Length) {
                    break;
                }

                c = json [index++];
                if (c == '"') {
                    complete = true;
                    break;
                } else if (c == '\\') {

                    if (index == json.Length) {
                        break;
                    }
                    c = json [index++];
                    if (c == '"') {
                        s.Append ('"');
                    } else if (c == '\\') {
                        s.Append ('\\');
                    } else if (c == '/') {
                        s.Append ('/');
                    } else if (c == 'b') {
                        s.Append ('\b');
                    } else if (c == 'f') {
                        s.Append ('\f');
                    } else if (c == 'n') {
                        s.Append ('\n');
                    } else if (c == 'r') {
                        s.Append ('\r');
                    } else if (c == 't') {
                        s.Append ('\t');
                    } else if (c == 'u') {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4) {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse (new string (json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))) {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append (Char.ConvertFromUtf32 ((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        } else {
                            break;
                        }
                    }

                } else {
                    s.Append (c);
                }

            }

            if (!complete) {
                success = false;
                return null;
            }

            return s.ToString ();
        }

        static double ParseNumber (char[] json, ref int index, ref bool success) {
            EatWhitespace (json, ref index);

            int lastIndex = GetLastIndexOfNumber (json, index);
            int charLength = (lastIndex - index) + 1;

            double number;
            success = Double.TryParse (new string (json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        static int GetLastIndexOfNumber (char[] json, int index) {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
                if ("0123456789+-.eE".IndexOf (json [lastIndex]) == -1) {
                    break;
                }
            }
            return lastIndex - 1;
        }

        static void EatWhitespace (char[] json, ref int index) {
            for (; index < json.Length; index++) {
                if (" \t\n\r".IndexOf (json [index]) == -1) {
                    break;
                }
            }
        }

        static int LookAhead (char[] json, int index) {
            int saveIndex = index;
            return NextToken (json, ref saveIndex);
        }

        static int NextToken (char[] json, ref int index) {
            EatWhitespace (json, ref index);

            if (index == json.Length) {
                return JsonParser.TOKEN_NONE;
            }

            char c = json [index];
            index++;
            switch (c) {
                case '{':
                    return JsonParser.TOKEN_CURLY_OPEN;
                case '}':
                    return JsonParser.TOKEN_CURLY_CLOSE;
                case '[':
                    return JsonParser.TOKEN_SQUARED_OPEN;
                case ']':
                    return JsonParser.TOKEN_SQUARED_CLOSE;
                case ',':
                    return JsonParser.TOKEN_COMMA;
                case '"':
                    return JsonParser.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JsonParser.TOKEN_NUMBER;
                case ':':
                    return JsonParser.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5) {
                if (json [index] == 'f' &&
                    json [index + 1] == 'a' &&
                    json [index + 2] == 'l' &&
                    json [index + 3] == 's' &&
                    json [index + 4] == 'e') {
                    index += 5;
                    return JsonParser.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4) {
                if (json [index] == 't' &&
                    json [index + 1] == 'r' &&
                    json [index + 2] == 'u' &&
                    json [index + 3] == 'e') {
                    index += 4;
                    return JsonParser.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4) {
                if (json [index] == 'n' &&
                    json [index + 1] == 'u' &&
                    json [index + 2] == 'l' &&
                    json [index + 3] == 'l') {
                    index += 4;
                    return JsonParser.TOKEN_NULL;
                }
            }

            return JsonParser.TOKEN_NONE;
        }

        static bool SerializeValue (object value, StringBuilder builder) {
            bool success = true;

            if (value is string) {
                success = SerializeString ((string)value, builder);
            } else if (value is Hashtable) {
                success = SerializeObject ((Hashtable)value, builder);
            } else if (value is ArrayList) {
                success = SerializeArray ((ArrayList)value, builder);
            } else if ((value is Boolean) && ((Boolean)value == true)) {
                builder.Append ("true");
            } else if ((value is Boolean) && ((Boolean)value == false)) {
                builder.Append ("false");
            } else if (value is ValueType) {
                // thanks to ritchie for pointing out ValueType to me
                success = SerializeNumber (Convert.ToDouble (value), builder);
            } else if (value == null) {
                builder.Append ("null");
            } else {
                success = false;
            }
            return success;
        }

        static bool SerializeObject (Hashtable anObject, StringBuilder builder) {
            builder.Append ("{");

            IDictionaryEnumerator e = anObject.GetEnumerator ();
            bool first = true;
            while (e.MoveNext ()) {
                string key = e.Key.ToString ();
                object value = e.Value;

                if (!first) {
                    builder.Append (", ");
                }

                SerializeString (key, builder);
                builder.Append (":");
                if (!SerializeValue (value, builder)) {
                    return false;
                }

                first = false;
            }

            builder.Append ("}");
            return true;
        }

        static bool SerializeArray (ArrayList anArray, StringBuilder builder) {
            builder.Append ("[");

            bool first = true;
            for (int i = 0; i < anArray.Count; i++) {
                object value = anArray [i];

                if (!first) {
                    builder.Append (", ");
                }

                if (!SerializeValue (value, builder)) {
                    return false;
                }

                first = false;
            }

            builder.Append ("]");
            return true;
        }

        static bool SerializeString (string aString, StringBuilder builder) {
            builder.Append ("\"");

            char[] charArray = aString.ToCharArray ();
            for (int i = 0; i < charArray.Length; i++) {
                char c = charArray [i];
                if (c == '"') {
                    builder.Append ("\\\"");
                } else if (c == '\\') {
                    builder.Append ("\\\\");
                } else if (c == '\b') {
                    builder.Append ("\\b");
                } else if (c == '\f') {
                    builder.Append ("\\f");
                } else if (c == '\n') {
                    builder.Append ("\\n");
                } else if (c == '\r') {
                    builder.Append ("\\r");
                } else if (c == '\t') {
                    builder.Append ("\\t");
                } else {
                    int codepoint = Convert.ToInt32 (c);
                    if ((codepoint >= 32) && (codepoint <= 126)) {
                        builder.Append (c);
                    } else {
                        builder.Append ("\\u" + Convert.ToString (codepoint, 16).PadLeft (4, '0'));
                    }
                }
            }

            builder.Append ("\"");
            return true;
        }

        static bool SerializeNumber (double number, StringBuilder builder) {
            builder.Append (Convert.ToString (number, CultureInfo.InvariantCulture));
            return true;
        }
    }
}
