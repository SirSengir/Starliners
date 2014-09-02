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

    public class IconSymbol : Widget {
        uint _index;
        Drawable _drawable;

        public IconSymbol (Vect2i position, Vect2i size, string symbol)
            : base (position, size, "symbol." + symbol) {
            _index = SpriteManager.Instance.RegisterSingle (symbol);
        }

        public IconSymbol (Vect2i position, Vect2i size, Drawable drawable)
            : base (position, size, "symbol.drawable") {
            _drawable = drawable;
        }

        public override void Draw (RenderTarget target, RenderStates states) {

            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);

            Drawable drawable = _drawable != null ? _drawable : SpriteManager.Instance [_index];
            states.Transform.Scale (Size / drawable.LocalBounds.Size);
            target.Draw (drawable, states);
        }
    }
}
