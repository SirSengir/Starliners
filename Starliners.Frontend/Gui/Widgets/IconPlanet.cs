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

﻿using System;
using BLibrary.Gui;
using BLibrary.Util;
using Starliners.Game;
using Starliners.Graphics;
using BLibrary.Graphics;
using Starliners.Game.Forces;
using Starliners.Game.Planets;

namespace Starliners.Gui.Widgets {
    sealed class IconPlanet : Widget {
        #region Constants

        static readonly Vect2i ICON_SIZE = new Vect2i (256, 256);

        #endregion

        IDataReference<EntityPlanet> _reference;

        public IconPlanet (Vect2i position, IDataReference<EntityPlanet> reference)
            : this (position, ICON_SIZE, reference) {
        }

        public IconPlanet (Vect2i position, Vect2i size, IDataReference<EntityPlanet> reference)
            : base (position, size) {
            _reference = reference;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative);
            float scale = (float)Size.X / ICON_SIZE.X;
            states.Transform.Scale (scale, scale);

            RendererPlanet.Instance.DrawRenderable (target, states, _reference.Value);
        }
    }
}

