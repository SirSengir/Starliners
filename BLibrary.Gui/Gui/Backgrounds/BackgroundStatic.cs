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

namespace BLibrary.Gui.Backgrounds {

    public sealed class BackgroundStatic : Background {
        Sprite _background;

        public BackgroundStatic (string name)
            : this (GuiManager.Instance.GetGuiSprite (name)) {
        }

        BackgroundStatic (Sprite sprite) {
            _background = sprite;
            Colour = Colour.White;
        }

        public override void Render (Vect2i position, Vect2i size, RenderTarget target, RenderStates states, Colour colour) {
            _background.Position = position;
            _background.Colour = colour;

            if (size.X != _background.SourceRect.Width || size.Y != _background.SourceRect.Height) {
                _background.Scale = new Vect2f ((float)size.X / _background.SourceRect.Width, (float)size.Y / _background.SourceRect.Height);
            }

            target.Draw (_background, states);
        }

        public override Background Copy () {
            return new BackgroundStatic (_background) { Colour = Colour, Shadow = Shadow };
        }
    }
}
