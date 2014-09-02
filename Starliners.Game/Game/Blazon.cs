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

using System;
using BLibrary.Serialization;
using System.Runtime.Serialization;
using BLibrary.Util;
using BLibrary.Json;

namespace Starliners.Game {

    public enum BlazonShape : byte {
        None = 0,
        Circle = 1,
        Shield = 2
    }

    public enum HeraldicStyle : byte {
        None = 0,
        Rotated90 = 1,
        Rotated180 = 2,
        Rotated270 = 3
    }

    [Serializable]
    public sealed class Blazon : ISerializable {
        #region Constants

        public static readonly Vect2i BASE_SIZE = new Vect2i (128, 128);
        public static readonly Vect2i DEFAULT_SIZE = new Vect2i (64, 64);
        public static readonly Colour[] VALID_COLOURS = new Colour[] {
            Colour.White, Colour.Black, Colour.Blue, Colour.Red, Colour.Green, Colour.Yellow, Colour.Purple, Colour.Orange,
            Colour.Olive, Colour.Moccasin, Colour.Amber, Colour.Beige, Colour.Chartreuse, Colour.Crimson, Colour.DeepPink,
            Colour.DodgerBlue, Colour.Gold, Colour.RoyalBlue
        };
        public static readonly string[] VALID_PATTERNS = new string[] {
            "vertical", "horizontal", "quartered", "stripe", "corners0", "diamond", "rhombus0"
        };
        public static readonly string[] VALID_HERALDICS = new string[] {
            "stars0", "stars1", "stars2", "circle", "money", "cross", "maltese", "equal", "question", "triangle", "triangles", "yinyan", "empire", "eye"
        };
        public static readonly HeraldicStyle[] VALID_STYLES = (HeraldicStyle[])Enum.GetValues (typeof(HeraldicStyle));

        #endregion

        /// <summary>
        /// A hopefully unique hashcode solely for use in rendering client side.
        /// </summary>
        /// <value>The cache code.</value>
        public int CacheCode {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the shape of this coat of arms.
        /// </summary>
        /// <value>The shape.</value>
        public BlazonShape Shape {
            get;
            private set;
        }

        /// <summary>
        /// Background colour.
        /// </summary>
        /// <value>The background.</value>
        public Colour Colour0 {
            get;
            set;
        }

        /// <summary>
        /// Foreground colour.
        /// </summary>
        /// <value>The foreground.</value>
        public Colour Colour1 {
            get;
            set;
        }

        /// <summary>
        /// The colour of the heraldic overlay.
        /// </summary>
        /// <value>The overlay.</value>
        public Colour Colour2 {
            get;
            set;
        }

        /// <summary>
        /// Indicates how backrgound and foreground are combined.
        /// </summary>
        /// <value>The pattern.</value>
        public string Pattern {
            get;
            set;
        }

        /// <summary>
        /// Indicates the heraldic symbol or animal.
        /// </summary>
        /// <value>The heraldic.</value>
        public string Heraldic {
            get;
            set;
        }

        /// <summary>
        /// Indicates the style used to draw the heraldic symbol to the CoA.
        /// </summary>
        /// <value>The style.</value>
        public HeraldicStyle Style {
            get;
            set;
        }

        #region Constructor

        public Blazon (BlazonShape shape) {
            Shape = shape;
        }

        public Blazon (IWorldAccess access, JsonObject json) {

            Colour[] colours = new Colour[3];

            Shape = json.ContainsKey ("shape") ? (BlazonShape)Enum.Parse (typeof(BlazonShape), json ["shape"].GetValue<string> (), true) : BlazonShape.Shield;
            Colour0 = json.ContainsKey ("colour0") ? new Colour (int.Parse (json ["colour0"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : SelectRandomColour (access.Rand, colours);
            Colour1 = json.ContainsKey ("colour1") ? new Colour (int.Parse (json ["colour1"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : SelectRandomColour (access.Rand, colours);
            Colour2 = json.ContainsKey ("colour2") ? new Colour (int.Parse (json ["colour2"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber)) : SelectRandomColour (access.Rand, colours);

            Pattern = json.ContainsKey ("pattern") ? json ["pattern"].GetValue<string> () : VALID_PATTERNS [access.Rand.Next (VALID_PATTERNS.Length)];
            Heraldic = json.ContainsKey ("heraldic") ? json ["heraldic"].GetValue<string> () : VALID_HERALDICS [access.Rand.Next (VALID_HERALDICS.Length)];
            Style = json.ContainsKey ("style") ? (HeraldicStyle)Enum.Parse (typeof(HeraldicStyle), json ["style"].GetValue<string> (), true) : HeraldicStyle.None;
        }

        #endregion

        #region Serialization

        public Blazon (SerializationInfo info, StreamingContext context) {
            Shape = (BlazonShape)info.GetByte ("Shape");
            Colour0 = new Colour (info.GetInt32 ("Colour0"));
            Colour1 = new Colour (info.GetInt32 ("Colour1"));
            Colour2 = new Colour (info.GetInt32 ("Colour2"));
            Pattern = info.GetString ("Pattern");
            Heraldic = info.GetString ("Heraldic");
            Style = (HeraldicStyle)info.GetByte ("Style");

            ResetCacheCode ();
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Shape", (byte)Shape);
            info.AddValue ("Colour0", Colour0.ToInteger ());
            info.AddValue ("Colour1", Colour1.ToInteger ());
            info.AddValue ("Colour2", Colour2.ToInteger ());
            info.AddValue ("Pattern", Pattern);
            info.AddValue ("Heraldic", Heraldic);
            info.AddValue ("Style", (byte)Style);
        }

        #endregion

        void ResetCacheCode () {
            CacheCode = 17;
            unchecked {
                CacheCode = CacheCode * 23 + Shape.GetHashCode ();
                CacheCode = CacheCode * 23 + Colour0.GetHashCode ();
                CacheCode = CacheCode * 23 + Colour1.GetHashCode ();
                CacheCode = CacheCode * 23 + Colour2.GetHashCode ();
                CacheCode = CacheCode * 23 + Pattern.GetHashCode ();
                CacheCode = CacheCode * 23 + Heraldic.GetHashCode ();
                CacheCode = CacheCode * 23 + Style.GetHashCode ();
            }
        }

        static Blazon _empty;

        public static Blazon GetEmpty (IWorldAccess access, BlazonShape shape) {
            if (_empty == null) {
                _empty = new Blazon (shape) {
                    Colour0 = Colour.DarkGray,
                    Colour1 = Colour.DarkGray,
                    Colour2 = Colour.DarkRed,
                    Pattern = VALID_PATTERNS [access.Rand.Next (VALID_PATTERNS.Length)],
                    Heraldic = "question"
                };
            }

            return _empty;
        }

        public static Blazon CreateRandom (IWorldAccess access, BlazonShape shape) {
            Colour[] colours = new Colour[3];
            for (int i = 0; i < colours.Length; i++)
                colours [i] = SelectRandomColour (access.Rand, colours);

            Blazon randomized = new Blazon (shape) {
                Colour0 = colours [0],
                Colour1 = colours [1],
                Colour2 = colours [2],
                Pattern = VALID_PATTERNS [access.Rand.Next (VALID_PATTERNS.Length)],
                Heraldic = VALID_HERALDICS [access.Rand.Next (VALID_HERALDICS.Length)],
                Style = VALID_STYLES [access.Rand.Next (VALID_STYLES.Length)]
            };
            return randomized;
        }

        static Colour SelectRandomColour (Random rand, Colour[] exclude) {
            Colour selected = VALID_COLOURS [rand.Next (VALID_COLOURS.Length)];

            // Ugly
            while (true) {
                selected = VALID_COLOURS [rand.Next (VALID_COLOURS.Length)];
                bool exists = false;
                for (int i = 0; i < exclude.Length; i++) {
                    if (exclude [i] != selected) {
                        continue;
                    }
                    exists = true;
                    break;
                }

                if (exists) {
                    continue;
                }

                break;
            }

            return selected;
        }
    }
}

