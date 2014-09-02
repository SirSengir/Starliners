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

using BLibrary.Util;
using Starliners;


namespace BLibrary.Gui {

    public sealed class WindowPresets {
        #region Properties

        public string Key {
            get;
            private set;
        }

        public ScreenGroup Group {
            get;
            set;
        }

        public Positioning Positioning {
            get;
            private set;
        }

        public Vect2i InnerArea {
            get;
            private set;
        }

        public string Style {
            get;
            set;
        }

        public bool SavesPosition {
            get;
            set;
        }

        public TabSaved SavedTab {
            get;
            set;
        }

        #endregion

        bool _headed;

        public WindowPresets (string key, Vect2i size, Positioning positioning, bool headed) {
            Key = key;
            InnerArea = size;
            Positioning = positioning;
            Group = ScreenGroup.Windows;
            _headed = headed;
            Style = "default";
            SavedTab = new TabSaved ();
        }

        public Vect2i GetOuterSize (IInterfaceDefinition uiProvider) {
            return InnerArea + (_headed ? new Vect2i (uiProvider.Margin.X * 2, uiProvider.Margin.Y * 2 - 5 + 40) : uiProvider.Margin * 2);
        }
    }
}

