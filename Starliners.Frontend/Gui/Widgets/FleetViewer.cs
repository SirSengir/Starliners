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
using BLibrary.Graphics.Sprites;
using Starliners.Game.Forces;

namespace Starliners.Gui.Widgets {
    sealed class FleetViewer : Widget {

        #region Constants

        static readonly Vect2i ICON_SIZE = new Vect2i (80, 80);

        #endregion

        IDataReference<ulong> _serial;

        public FleetViewer (Vect2i position, Vect2i size, IDataReference<ulong> serial)
            : base (position, size) {
            _serial = serial;
        }

        Fleet GetFleet () {
            return GameAccess.Interface.Local.RequireState<Fleet> (_serial.Value);
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Fleet fleet = GetFleet ();
            AddWidget (new Canvas (Vect2i.ZERO, Size, "starfield3"));

            int[] counts = new int[ShipSizes.VALUES.Length - 1];
            foreach (Levy levy in fleet.Levies) {
                levy.Census (counts);
            }
            int[] hashes = new int[ShipSizes.VALUES.Length - 1];
            for (int i = 0; i < hashes.Length; i++) {
                hashes [i] = GameAccess.Interface.Local.Rand.Next ();
            }

            Grouping grouped = new Grouping (Vect2i.ZERO, Size) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            foreach (ShipSize size in ShipSizes.VALUES) {
                if (size == ShipSize.None) {
                    continue;
                }
                int ordinal = (int)size - 1;
                if (counts [ordinal] <= 0) {
                    continue;
                }

                int row = ordinal % 2;
                int column = ordinal / 2;

                ShipProjector projector = new ShipProjector (size.ToString ().ToLowerInvariant () + "0", fleet.Backer.Colours.Vessels, fleet.Backer.Colours.Shields, hashes [ordinal]);
                projector.RegisterIcons (SpriteManager.Instance);

                Vect2i start = new Vect2i (column * 160, row * 32);
                grouped.AddWidget (new IconVessel (start, ICON_SIZE, projector));
                grouped.AddWidget (new Label (start + new Vect2i (80, 32), new Vect2i (80, 32), string.Format ("§{0}?+b§x {1}", Colour.Yellow.ToString ("#"), counts [ordinal])));
            }
        }

    }
}

