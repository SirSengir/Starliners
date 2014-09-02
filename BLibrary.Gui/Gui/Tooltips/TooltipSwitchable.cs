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
using BLibrary.Graphics.Text;
using BLibrary.Util;
using BLibrary.Gui.Widgets;

namespace BLibrary.Gui.Tooltips {

    public class TooltipSwitchable : Tooltip {
        Switchable _switchable;
        int _lastSelect;
        TextBuffer _buffer;

        public TooltipSwitchable (Switchable switchable)
            : base (new Vect2i (256, 128)) {
            _switchable = switchable;
            AddHeader (new TextComponent ("--"), 0);
            _buffer = new TextBuffer ("--");
            _buffer.SetMaxWidth (MAX_TOOLTIP_WIDTH);
            RegenLines ();
        }

        void RegenLines () {
            SetHeader (_switchable.TooltipHeader [_switchable.Selected]);
            _buffer.SetLines (new string[] {
                _switchable.TooltipInfo [_switchable.Selected]
            });
        }

        protected override Vect2i GetDimensions () {
            return (Vect2i)(_buffer.LocalBounds.Size);
        }

        public override void Draw (RenderTarget target, RenderStates states) {

            if (_lastSelect != _switchable.Selected) {
                RegenLines ();
                _lastSelect = _switchable.Selected;
            }

            base.Draw (target, states);
            states.Transform.Translate (PositionRelative + CornerTopLeft);
            _buffer.Draw (target, states);

        }
    }
}

