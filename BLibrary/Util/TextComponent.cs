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

﻿using BLibrary.Resources;
using System;
using System.Runtime.Serialization;
using BLibrary.Util;

namespace BLibrary.Util {

    /// <summary>
    /// Represents a potentially localizable fragment of text.
    /// </summary>
    [Serializable]
    public sealed class TextComponent : ISerializable, ITextProvider {
        #region Properties

        public long LastUpdated {
            get {
                return 0;
            }
        }

        public string Text {
            get;
            private set;
        }

        public string Template {
            get;
            set;
        }

        public bool IsLocalizable {
            get;
            set;
        }

        #endregion

        #region Constructor

        public TextComponent (string text) {
            Text = text;
        }

        #endregion

        #region Serialization

        public TextComponent (SerializationInfo info, StreamingContext context) {
            Text = info.GetString ("Text");
            Template = info.GetString ("Template");
            IsLocalizable = info.GetBoolean ("Localizable");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Text", Text);
            info.AddValue ("Template", Template);
            info.AddValue ("Localizable", IsLocalizable);
        }

        #endregion

        public override string ToString () {
            if (!string.IsNullOrEmpty (Template)) {
                return IsLocalizable ? string.Format (Template, Localization.Instance [Text]) : string.Format (Template, Text);
            } else {
                return IsLocalizable ? Localization.Instance [Text] : Text;
            }
        }

        public static TextComponent[] ConvertToComponents (string[] lines) {
            TextComponent[] components = new TextComponent[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                components [i] = new TextComponent (lines [i]);
            }
            return components;
        }

        public static string[] ConvertToStrings (ITextProvider[] lines) {
            string[] components = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                components [i] = lines [i].ToString ();
            }
            return components;
        }
    }
}
