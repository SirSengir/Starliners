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
using BLibrary.Util;
using BLibrary.Gui.Backgrounds;

namespace BLibrary.Gui.Widgets {

    public class Gauge : Widget {
        float _positive = 1f;
        float _negative = 1f;
        float _median = 1f;
        Background _grid;
        Background _med;
        Background _neg;
        Background _pos;

        public Gauge (Vect2i position, Vect2i size, string key, float positive, float negative, float median, float max)
            : base (position, size, key) {

            _positive = positive / max;
            _negative = negative / max;
            _median = median / max;

            _grid = new BackgroundStatic ("guiGauge");
            _med = new BackgroundSimple (Colour.LawnGreen);
            _neg = new BackgroundSimple (Colour.DeepSkyBlue);
            _pos = new BackgroundSimple (Colour.OrangeRed);
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            int halfSize = Size.X / 2;

            states.Transform.Translate (PositionRelative);
            // Negative
            int barX = (int)(_negative * halfSize);
            int shift = (int)(_median * halfSize);
            if (_negative > 0.01f) {
                _neg.Render (new Vect2i (halfSize + shift - barX, 0), new Vect2i (barX, Size.Y), target, states, _neg.Colour);
            }

            // Positive
            if (_positive > 0.01f) {
                _pos.Render (new Vect2i (halfSize + shift, 0), new Vect2i ((int)(_positive * halfSize), Size.Y), target, states, _pos.Colour);
            }

            // Median
            _med.Render (new Vect2i (halfSize + shift - 1, 0), new Vect2i (2, Size.Y), target, states, _med.Colour);

            _grid.Render (Size, target, states);
        }
    }
}
