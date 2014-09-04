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
using Starliners.Game;
using BLibrary.Util;
using BLibrary.Graphics;
using Starliners.Graphics;
using BLibrary.Gui.Widgets;
using BLibrary.Graphics.Text;
using BLibrary.Resources;
using Starliners.Game.Planets;

namespace Starliners.Gui.Widgets {
    sealed class PlanetViewer : Widget {

        static readonly Vect2i BLAZON_SIZE = new Vect2i (48, 48);

        IDataReference<ulong> _serial;
        IDataReference<float> _loyality;

        public PlanetViewer (Vect2i position, Vect2i size, IDataReference<ulong> serial, IDataReference<float> loyality)
            : base (position, size) {
            _serial = serial;
            _loyality = loyality;
        }

        EntityPlanet GetPlanet () {
            return GameAccess.Interface.Local.RequireEntity<EntityPlanet> (_serial.Value);
        }

        protected override void Regenerate () {
            base.Regenerate ();

            EntityPlanet planet = GetPlanet ();

            AddWidget (new Canvas (Vect2i.ZERO, Size, "starfield3"));

            int measure = Size.X / 10;
            int pwidth = 4 * measure;

            AddWidget (new IconPlanet (new Vect2i ((pwidth - 32) / 2, Size.Y / 2), new Vect2i (96, 96), new DataPod<EntityPlanet> (planet)));
            AddWidget (new IconBlazon (new Vect2i (UIProvider.MarginSmall.X, Size.Y - BLAZON_SIZE.Y - UIProvider.MarginSmall.Y), BLAZON_SIZE, planet.Owner));

            Grouping info = new Grouping (new Vect2i (pwidth, 0), new Vect2i (measure * 6, Size.Y)) { AlignmentV = Alignment.Center };
            AddWidget (info);

            Vect2i start = Vect2i.ZERO;

            Label type = new Label (start, string.Format ("§{0}?+b§{1}: {2}", Colour.Amber.ToString ("#"), Localization.Instance ["planetary_type"], Localization.Instance [string.Format ("type_{0}", planet.PlanetData.Type.ToString ().ToLowerInvariant ())]));
            info.AddWidget (type);

            start += new Vect2i (0, type.Size.Y);
            Label culture = new Label (start, string.Format ("§{0}?+b§{1}: {2}", Colour.LightGray.ToString ("#"), Localization.Instance ["planetary_culture"], Localization.Instance [planet.PlanetData.Culture.Name]));
            info.AddWidget (culture);

            start += new Vect2i (0, culture.Size.Y);
            Label loyality = new Label (start, "§{0}?+b§{1}: {2}", Colour.Green.ToString ("#"), Localization.Instance ["planetary_loyality"], _loyality);
            info.AddWidget (loyality);
        }
    }
}

