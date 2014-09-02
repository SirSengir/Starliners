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
using Starliners.Game.Planets;
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using BLibrary.Gui.Backgrounds;

namespace Starliners.Graphics {
    sealed class LegendPlanet : LegendElement {

        EntityPlanet _planet;

        public LegendPlanet (EntityPlanet planet)
            : base (planet, new Vect2i (256, 56)) {
            _planet = planet;

            Anchored = Anchor.TopCenter;
            CoordinateOffset = new Vect2d (0, 1);
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Grouping grouped = new Grouping (Vect2i.ZERO, Size) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            grouped.AddWidget (new Label (new Vect2i (16, 8), new Vect2i (Size.X - 32, 40), _planet.PlanetData.FullName) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center,
                Backgrounds = new BackgroundCollection (GuiManager.Instance.UIProvider.Backgrounds ["map.plaque"].Copy (_planet.Owner.Colours.Empire))
            });
            grouped.AddWidget (new IconBlazon (Vect2i.ZERO, new Vect2i (Size.Y, Size.Y), _planet.Owner));
        }

        protected override void Refresh () {
            base.Refresh ();
            IsGenerated = false;
        }
    }
}

