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

﻿using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;


namespace BLibrary.Gui.Widgets {

    public class Canvas : Widget {

        uint _index;
        Drawable _image;

        public Canvas (Vect2i position, Vect2i size, string ident)
            : this (position, size, SpriteManager.Instance.RegisterSingle (ident)) {
        }

        public Canvas (Vect2i position, Vect2i size, uint index)
            : base (position, size) {
            _index = index;
        }

        public Canvas (Vect2i position, Vect2i size, Drawable image)
            : base (position, size) {
            _image = image;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            Drawable drawable = _image ?? SpriteManager.Instance [_index];
            states.Transform.Translate (PositionRelative);
            states.Transform.Scale ((Vect2f)Size / drawable.LocalBounds.Size);
            target.Draw (drawable, states);
        }
    }
}
