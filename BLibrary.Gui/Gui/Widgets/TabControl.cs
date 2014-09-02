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

﻿using System.Collections.Generic;
using System.Linq;
using BLibrary.Resources;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Gui.Widgets {

    public class TabControl : Widget {
        public IEnumerable<Frame> Tabs {
            get {
                return _tabs.Values;
            }
        }

        Dictionary<string, Frame> _tabs = new Dictionary<string, Frame> ();

        public TabControl (Vect2i position, Vect2i size, string key)
            : base (position, size, key) {
        }

        public void AddTab (Frame tab) {
            _tabs [tab.Key] = tab;
            AddWidget (tab);
        }

        public void SelectFirstTab () {
            UnselectAll ();
            _tabs.Values.First ().IsDisplayed = true;
        }

        public void SelectTab (string ident) {
            if (string.IsNullOrWhiteSpace (ident) || !_tabs.ContainsKey (ident)) {
                SelectFirstTab ();
                return;
            }
            UnselectAll ();
            _tabs [ident].IsDisplayed = true;
        }

        public void UnselectAll () {
            foreach (Frame tab in _tabs.Values)
                tab.IsDisplayed = false;
        }

        public Frame CreateToolbar (Vect2i start, Vect2i size) {
            Grouping toolbar = new Grouping (start, size) { Backgrounds = UIProvider.Style.CreateInset () };

            // Add buttons
            Frame[] tabs = Tabs.ToArray ();
            for (int i = 0; i < tabs.Length; i++) {
                toolbar.AddWidget (new Button (new Vect2i (4, 4) + new Vect2i (0, 26) * i, new Vect2i (toolbar.Size.X - 8, 24), Constants.CONTAINER_KEY_TAB_PAGE_SELECT, Localization.Instance [tabs [i].Key.Replace ('.', '_')], tabs [i].Key));
            }

            return toolbar;
        }
    }
}
