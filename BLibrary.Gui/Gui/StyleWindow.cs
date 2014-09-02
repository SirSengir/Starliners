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

﻿using BLibrary.Json;
using System.Collections.Generic;
using Starliners;
using BLibrary.Gui.Backgrounds;

namespace BLibrary.Gui {

    public sealed class StyleWindow : StyleElement {

        public string Key {
            get;
            private set;
        }

        public StyleHeader HeaderStyle {
            get;
            private set;
        }

        public StyleControl ButtonStyle {
            get;
            private set;
        }

        public StyleControl SwitchableStyle {
            get;
            private set;
        }

        public StyleControl TabStyle {
            get;
            private set;
        }

        public StyleInput InputStyle {
            get;
            private set;
        }

        public Dictionary<string, StyleControl> SlotStyles {
            get { return _slots; }
        }

        #region Fields

        Background _background;
        Background _inset;
        Dictionary<string, StyleControl> _slots = new Dictionary<string, StyleControl> ();

        #endregion

        public StyleWindow (IInterfaceDefinition provider, JsonObject json)
            : base (provider, json) {

            Key = json ["key"].GetValue<string> ();
            _background = provider.Backgrounds [json ["background"].GetValue<string> ()];
            _inset = provider.Backgrounds [json ["inset"].GetValue<string> ()];

            HeaderStyle = new StyleHeader (provider, json ["header"].GetValue<JsonObject> ());
            ButtonStyle = new StyleControl (provider, json ["button"].GetValue<JsonObject> ());
            SwitchableStyle = new StyleControl (provider, json ["switchable"].GetValue<JsonObject> ());
            TabStyle = new StyleControl (provider, json ["tab"].GetValue<JsonObject> ());
            InputStyle = new StyleInput (provider, json ["input"].GetValue<JsonObject> ());

            foreach (JsonObject obj in json["slots"].AsEnumerable<JsonObject>()) {
                _slots [obj ["key"].GetValue<string> ()] = new StyleControl (provider, obj);
            }
        }

        public BackgroundCollection CreateInset () {
            return new BackgroundCollection (_inset.Copy ());
        }

        public BackgroundCollection CreateBackgrounds () {
            return new BackgroundCollection (_background.Copy ());
        }
    }
}

