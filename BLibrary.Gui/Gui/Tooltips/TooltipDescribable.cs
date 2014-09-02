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

﻿using System.Collections.Generic;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using Starliners.Game;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Gui.Tooltips {

    public sealed class TooltipDescribable : Tooltip {
        TextBuffer _buffer;

        public TooltipDescribable (IDescribable describable)
            : base (new Vect2i (256, 64)) {

            AddHeader (new TextComponent (describable.Description), 0);

            IList<string> info = CombineWithSeperation (describable.GetInformation (GameAccess.Interface.ThePlayer), describable.GetUsage (GameAccess.Interface.ThePlayer));
            if (info.Count > 0) {
                _buffer = new TextBuffer (info);
                _buffer.SetMaxWidth (MAX_TOOLTIP_WIDTH);
            } else {
                HeaderOnly = true;
            }

        }

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
