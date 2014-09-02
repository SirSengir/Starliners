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
using Starliners.Game;
using Starliners.Graphics;

namespace BLibrary.Gui.Widgets {

    public sealed class IconBlazon : Widget {
        public BlazonShape DefaultShape {
            get;
            set;
        }

        IHeralded _heralded;
        Blazon _blazon;
        bool _useDressup;

        public IconBlazon (Vect2i position, Vect2i size, IHeralded heralded)
            : base (position, size, "blazon." + (heralded != null ? heralded.Serial.ToString () : LibraryConstants.NULL_ID.ToString ())) {

            DefaultShape = BlazonShape.Shield;
            _heralded = heralded;
        }

        public IconBlazon (Vect2i position, Vect2i size, Blazon blazon)
            : base (position, size, "blazon") {

            DefaultShape = BlazonShape.Shield;
            _blazon = blazon;
        }

        public IconBlazon (Vect2i position, Vect2i size)
            : base (position, size, "blazon.dressup") {
            DefaultShape = BlazonShape.Shield;
            _useDressup = true;
        }

        public override void Draw (RenderTarget target, RenderStates states) {

            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);

            states.Transform.Scale (Size / Blazon.BASE_SIZE);
            if (!_useDressup) {
                if (_heralded != null) {
                    RendererBlazon.Instance.DrawRenderable (target, states, _heralded, DefaultShape);
                } else {
                    RendererBlazon.Instance.DrawRenderable (target, states, _blazon, DefaultShape);
                }
            } else {
                RendererBlazon.Instance.DrawDressup (target, states, DefaultShape);
            }
        }
    }
}

