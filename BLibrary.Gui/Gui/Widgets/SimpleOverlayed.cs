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
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;


namespace BLibrary.Gui.Widgets {

    public class SimpleOverlayed : Widget {
        IDataReference<float> _keyed;
        Sprite _base;
        Sprite _overlay;

        public SimpleOverlayed (Vect2i position, Vect2i size, string key, string ident, IDataReference<float> keyed)
            : base (position, size, key) {

            _keyed = keyed;

            Sprite texture = GuiManager.Instance.GetGuiSprite (ident);
            int width = texture.SourceRect.Size.X / 2;
            _base = new Sprite (texture);
            _base.SourceRect = new Rect2i (texture.SourceRect.Coordinates, width, texture.SourceRect.Size.Y);
            _overlay = new Sprite (texture);
            _overlay.SourceRect = new Rect2i (texture.SourceRect.Coordinates + new Vect2i (width, 0), width, texture.SourceRect.Size.Y);
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative);
            target.Draw (_base, states);

            int height = (int)(_base.SourceRect.Height * _keyed.Value);
            if (height <= 0)
                return;

            states.Transform.Translate (0, _base.SourceRect.Height - height);
            _overlay.SourceRect = new Rect2i (_base.SourceRect.Coordinates + new Vect2i (_base.SourceRect.Width, _base.SourceRect.Height - height), _base.SourceRect.Width, height);
            target.Draw (_overlay, states);
        }
    }
}
