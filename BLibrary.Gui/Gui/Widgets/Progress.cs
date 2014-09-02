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

    public class Progress : Widget {

        IDataReference<float> _reference;
        Sprite _template;
        Sprite _cell;
        Rect2i[] stages;

        public Progress (Vect2i position, Vect2i size, IDataReference<float> reference)
            : this (position, size, reference, "guiProgress") {
        }

        public Progress (Vect2i position, Vect2i size, IDataReference<float> reference, string texture)
            : base (position, size) {

            _reference = reference;
            _template = new Sprite (GuiManager.Instance.GetGuiSprite (texture));

            stages = new Rect2i[6];
            for (int i = 0; i < 6; i++) {
                stages [i] = new Rect2i ((Vect2i)(_template.SourceRect.Coordinates + new Vect2i (i * 8, 0)), 8, _template.SourceRect.Height);
                if (i == 0) {
                    _cell = new Sprite (_template);
                    _cell.SourceRect = stages [i];
                }
            }

        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            float progressed = _reference.Value;
            states.Transform.Translate (PositionRelative);

            float scaleY = (float)Size.Y / _cell.SourceRect.Height;
            states.Transform.Scale (Size.X / _cell.SourceRect.Width, scaleY);
            _cell.Colour = Tinting.GetTint (this, Colour.White);

            target.Draw (_cell, states);

            Rect2i rect;
            if (progressed > 0.8) {
                rect = stages [5];
            } else if (progressed > 0.6) {
                rect = stages [4];
            } else if (progressed > 0.4) {
                rect = stages [3];
            } else if (progressed > 0.2) {
                rect = stages [2];
            } else {
                rect = stages [1];
            }

            int fill = (int)((1.0f - progressed) * rect.Height);
            _template.SourceRect = new Rect2i (rect.Left, rect.Top + fill, rect.Width, rect.Height - fill);
            states.Transform.Translate (0, fill);
            target.Draw (_template, states);
        }

    }
}
