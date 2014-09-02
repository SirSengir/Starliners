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
using BLibrary.Util;
using BLibrary.Gui.Widgets;


namespace BLibrary.Gui {

    abstract class GuiText : GuiLocal {
        const string WINDOW_KEY = "menu_text";
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, new Vect2i (776, 541), Positioning.Centered, true);
        ResourceFile _resource;
        string _header;
        Alignment _hAlign;

        public GuiText (ResourceFile resource, string header, Alignment hAlign)
            : base (WINDOW_SETTING) {

            _resource = resource;
            _header = header;
            _hAlign = hAlign;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            int margin = UIProvider.Margin.X;

            AddHeader (WindowButton.None, _header);

            Vect2i framesize = new Vect2i (Size.X - 2 * margin, Size.Y - 2 * margin - 60 - 40);
            Grouping toolbar = new Grouping (CornerTopLeft, framesize) { Backgrounds = UIProvider.Style.CreateInset () };
            AddWidget (toolbar);

            toolbar.AddWidget (new RichTextView (new Vect2i (16, 16), framesize - new Vect2i (32, 32), "about.text", _resource) { AlignmentH = _hAlign });
        }
    }
}
