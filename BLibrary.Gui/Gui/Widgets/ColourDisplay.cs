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

    public class ColourDisplay : Widget {

        Sprite _cell;
        Sprite _colour;

        public ColourDisplay (Vect2i position, Vect2i size, string key, Colour colour)
            : base (position, size, key) {

            Sprite template = GuiManager.Instance.GetGuiSprite ("guiCheckbox");

            _cell = new Sprite (template);
            int cellWidth = template.SourceRect.Width / 2;
            _cell.SourceRect = new Rect2i (template.SourceRect.Coordinates + new Vect2i (cellWidth, 0), cellWidth, template.SourceRect.Height);

            _colour = new Sprite (template);
            _colour.SourceRect = new Rect2i (template.SourceRect.Coordinates, cellWidth, template.SourceRect.Height);
            _colour.Colour = colour;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative);
            target.Draw (_colour, states);
            target.Draw (_cell, states);
        }

    }
}
