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


namespace BLibrary.Gui.Tooltips {

    public class TooltipSimple : Tooltip {
        TextBuffer _buffer;

        #region Constructor

        public TooltipSimple (string header, params string[] info)
            : base (new Vect2i (256, 128)) {

            AddHeader (new TextComponent (header), 0);

            if (info.Length > 0) {
                _buffer = new TextBuffer (info);
                _buffer.SetMaxWidth (MAX_TOOLTIP_WIDTH);
            } else {
                HeaderOnly = true;
            }
        }

        #endregion

        protected override Vect2i GetDimensions () {
            return _buffer != null ? (Vect2i)(_buffer.LocalBounds.Size) : new Vect2i ();
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);
            if (_buffer == null) {
                return;
            }
            states.Transform.Translate (PositionRelative + CornerTopLeft);
            _buffer.Draw (target, states);
        }
    }
}
