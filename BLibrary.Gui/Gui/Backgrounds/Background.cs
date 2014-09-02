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

namespace BLibrary.Gui.Backgrounds {

    public abstract class Background {
        #region Properties

        public Colour Colour {
            get;
            set;
        }

        public Background Shadow {
            get;
            set;
        }

        #endregion

        public Background () {
            Colour = Colour.White;
        }

        public void Render (Vect2i size, RenderTarget target, RenderStates states, GuiElement element) {
            Render (Vect2i.ZERO, size, target, states, element);
        }

        public void Render (Vect2i size, RenderTarget target, RenderStates states) {
            Render (Vect2i.ZERO, size, target, states, Colour);
        }

        public void Render (Vect2i position, Vect2i size, RenderTarget target, RenderStates states, GuiElement element) {
            Render (position, size, target, states, element.Tinting.GetTint (element, Colour));
        }

        public void Render (Vect2i position, Vect2i size, RenderTarget target, RenderStates states) {
            Render (position, size, target, states, Colour);
        }

        public virtual void Render (Vect2i size, RenderTarget target, RenderStates states, Colour colour) {
            Render (Vect2i.ZERO, size, target, states, colour);
        }

        public virtual void Render (Vect2i position, Vect2i size, RenderTarget target, RenderStates states, Colour colour) {
            if (Shadow != null) {
                Shadow.Render (position + new Vect2i (8, 8), size, target, states);
            }
        }

        public abstract Background Copy ();

        public Background Copy (Colour colour) {
            Background copy = Copy ();
            copy.Colour = colour;
            return copy;
        }
    }
}
