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

﻿using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class ListItemDrawable : Widget {
        Widget _icon;
        readonly string _text;

        public ListItemDrawable (Vect2i position, Vect2i size, string key, Widget icon, string text)
            : base (position, size, key) {
            _icon = icon;
            _text = text;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            int padding = (Size.Y - _icon.Size.Y) / 2;
            _icon.PositionRelative = new Vect2i (padding, padding);
            AddWidget (_icon);

            Vect2i textstart = new Vect2i (padding + _icon.Size.X + (padding >= 6 ? padding : 6), 0);
            AddWidget (new Label (textstart, new Vect2i (Size.X - textstart.X, Size.Y), _text) {
                AlignmentV = Alignment.Center
            });

        }
    }
}

