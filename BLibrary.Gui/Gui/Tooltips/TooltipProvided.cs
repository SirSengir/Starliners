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
using BLibrary.Graphics.Text;

namespace BLibrary.Gui.Tooltips {

    public class TooltipProvided : Tooltip {
        static readonly ITextProvider[] NO_INFO = new ITextProvider[0];

        #region Fields

        ITextProvider[] _info;
        TextBuffer _buffer;

        #endregion

        #region Constructor

        public TooltipProvided (string header)
            : this (new TextComponent (header), NO_INFO) {
        }

        public TooltipProvided (string header, string info)
            : this (new TextComponent (header), new ITextProvider[] { new TextComponent (info) }) {
        }

        public TooltipProvided (string header, ITextProvider[] info)
            : this (new TextComponent (header), info) {
        }

        public TooltipProvided (ITextProvider header, ITextProvider[] info)
            : base (new Vect2i (256, 128)) {

            AddHeader (header, 0);
            _info = info;

            if (info.Length > 0) {
                _buffer = new TextBuffer (TextComponent.ConvertToStrings (_info));
                _buffer.SetMaxWidth (MAX_TOOLTIP_WIDTH);
                foreach (ITextProvider ifo in info) {
                    Subscribe (ifo);
                }
            } else {
                HeaderOnly = true;
            }
        }

        #endregion

        protected override void Refresh () {
            base.Refresh ();
            _buffer.SetLines (TextComponent.ConvertToStrings (_info));
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

