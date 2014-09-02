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
using Starliners.Game.Forces;
using BLibrary.Resources;
using Starliners.Gui.Widgets;
using BLibrary.Gui.Widgets;
using System.Collections.Generic;
using BLibrary.Graphics.Text;

namespace Starliners.Gui.Tooltips {
    sealed class TooltipShip : Tooltip {

        static readonly Vect2i ICON_SIZE = new Vect2i (24, 24);

        int _slot;
        IDataReference<BattleGrid> _grid;
        IDataReference<ShipInstance> _ship;

        DataPod<float> _scShield;
        DataPod<float> _scArmour;
        DataPod<float> _scHull;

        DataPod<string> _txShield;
        DataPod<string> _txArmour;
        DataPod<string> _txHull;

        public TooltipShip (IDataReference<ShipInstance> ship)
            : base (new Vect2i (256, 128)) {
            _ship = ship;
            AddHeader ("<No Ship>", 0);

            Subscribe (ship);
        }

        public TooltipShip (int slot, IDataReference<BattleGrid> grid)
            : base (new Vect2i (256, 128)) {

            _slot = slot;
            _grid = grid;
            AddHeader ("<No Ship>", 0);

            Subscribe (grid);
        }

        protected override Vect2i GetDimensions () {
            return _ship != null || _grid.Value [_slot] != null ? new Vect2i (256, 128 + 72 + 48 + 100 + 72 + 4 * TOOLTIP_PADDING.Y) : Vect2i.ZERO;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            ShipInstance ship = _ship != null ? _ship.Value : _grid.Value [_slot];
            if (ship == null) {
                return;
            }
            SetHeader (Localization.Instance [ship.ShipClass.Name]);

            Vect2i start = CornerTopLeft;
            // Ship display
            Vect2i shipbox = new Vect2i (Size.X - 2 * TOOLTIP_PADDING.X, 128);
            AddWidget (new Canvas (start, shipbox, "starfield1"));
            AddWidget (new IconVessel (start - new Vect2i (0, 16), shipbox, ship.Projector));
            AddWidget (new IconSymbol (new Vect2i (Size.X - TOOLTIP_PADDING.X - 64, start.Y + shipbox.Y - 64), new Vect2i (48, 48), string.Format ("symbol{0}", ship.Level)));

            start += new Vect2i (0, shipbox.Y + TOOLTIP_PADDING.Y);
            Vect2i scalesize = new Vect2i (108, 18);

            // Health information
            Grouping grouped = new Grouping (start + new Vect2i (128 - TOOLTIP_PADDING.X, 0), new Vect2i (128, 72)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new Scale (Vect2i.ZERO, scalesize, 6, 1.0f, _scShield = new DataPod<float> ((float)ship.LayerShield / ship.ShipClass.Shield)) {
                FillColour = Colour.Turquoise,
                Text = new Label (Vect2i.ZERO, _txShield = new DataPod<string> (ship.LayerShield.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });
            grouped.AddWidget (new Scale (new Vect2i (0, 24), scalesize, 6, 1.0f, _scArmour = new DataPod<float> ((float)ship.LayerArmour / ship.ShipClass.Armour)) {
                FillColour = Colour.GoldenRod,
                Text = new Label (Vect2i.ZERO, _txArmour = new DataPod<string> (ship.LayerArmour.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });
            grouped.AddWidget (new Scale (new Vect2i (0, 48), scalesize, 6, 1.0f, _scHull = new DataPod<float> ((float)ship.LayerHull / ship.ShipClass.Hull)) {
                FillColour = Colour.LawnGreen,
                Text = new Label (Vect2i.ZERO, _txHull = new DataPod<string> (ship.LayerHull.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });

            AddWidget (new Label (start, "Shield:"));
            AddWidget (new Label (start + new Vect2i (0, 24), "Armour:"));
            AddWidget (new Label (start + new Vect2i (0, 48), "Hull:"));

            // Manouver & Tracking
            start += new Vect2i (0, 72 + TOOLTIP_PADDING.Y);
            grouped = new Grouping (start + new Vect2i (128 - TOOLTIP_PADDING.X, 0), new Vect2i (128, 72)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new Scale (Vect2i.ZERO, scalesize, 6, 1.0f, (float)ship.ShipClass.Manouver / ShipClass.MAX_MANOUVER) {
                FillColour = Colour.Amber,
                Text = new Label (Vect2i.ZERO, new DataPod<string> (ship.ShipClass.Manouver.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });
            grouped.AddWidget (new Scale (new Vect2i (0, 24), scalesize, 6, 1.0f, (float)ship.ShipClass.Tracking / ShipClass.MAX_TRACKING) {
                FillColour = Colour.Amber,
                Text = new Label (Vect2i.ZERO, new DataPod<string> (ship.ShipClass.Tracking.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });

            AddWidget (new Label (start, "Manouver:"));
            AddWidget (new Label (start + new Vect2i (0, 24), "Tracking:"));

            // Resistances
            start += new Vect2i (0, 48 + TOOLTIP_PADDING.Y);
            grouped = new Grouping (start + new Vect2i (96 - TOOLTIP_PADDING.X, 0), new Vect2i (160, 100)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            grouped.AddWidget (new IconSymbol (new Vect2i (12, 0), ICON_SIZE, "symbolHeat"));
            grouped.AddWidget (new IconSymbol (new Vect2i (60, 0), ICON_SIZE, "symbolKinetic"));
            grouped.AddWidget (new IconSymbol (new Vect2i (108, 0), ICON_SIZE, "symbolRadioactive"));

            AddResistInformation (new Vect2i (0, 28), grouped, ship.Properties.ResistsShield);
            AddResistInformation (new Vect2i (0, 52), grouped, ship.Properties.ResistsArmour);
            AddResistInformation (new Vect2i (0, 76), grouped, ship.Properties.ResistsHull);

            AddWidget (new Label (start + new Vect2i (0, 28), "Shield:"));
            AddWidget (new Label (start + new Vect2i (0, 52), "Armour:"));
            AddWidget (new Label (start + new Vect2i (0, 76), "Hull:"));

            // Weapon information
            start += new Vect2i (0, 100 + TOOLTIP_PADDING.Y);
            grouped = new Grouping (start + new Vect2i (128 - TOOLTIP_PADDING.X, 0), new Vect2i (128, 72)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            int symoff = (128 - TOOLTIP_PADDING.X - ICON_SIZE.X) / 2;
            AddWidget (new IconSymbol (start + new Vect2i (symoff, 0), ICON_SIZE, "symbolHeat"));
            AddWidget (new IconSymbol (start + new Vect2i (symoff, 24), ICON_SIZE, "symbolKinetic"));
            AddWidget (new IconSymbol (start + new Vect2i (symoff, 48), ICON_SIZE, "symbolRadioactive"));

            grouped.AddWidget (new Scale (Vect2i.ZERO, scalesize, 6, 1.0f, (float)ship.Properties.FireHeat / ship.Properties.FullBarrage) {
                BaseLine = (float)ship.ShipClass.FireHeat / ship.Properties.FullBarrage,
                FillColour = Colour.Yellow,
                Text = new Label (Vect2i.ZERO, new DataPod<string> (ship.Properties.FireHeat.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });
            grouped.AddWidget (new Scale (new Vect2i (0, 24), scalesize, 6, 1.0f, (float)ship.Properties.FireKinetic / ship.Properties.FullBarrage) {
                BaseLine = (float)ship.ShipClass.FireKinetic / ship.Properties.FullBarrage,
                FillColour = Colour.Yellow,
                Text = new Label (Vect2i.ZERO, new DataPod<string> (ship.Properties.FireKinetic.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });
            grouped.AddWidget (new Scale (new Vect2i (0, 48), scalesize, 6, 1.0f, (float)ship.Properties.FireRadiation / ship.Properties.FullBarrage) {
                BaseLine = (float)ship.ShipClass.FireRadiation / ship.Properties.FullBarrage,
                FillColour = Colour.Yellow,
                Text = new Label (Vect2i.ZERO, new DataPod<string> (ship.Properties.FireRadiation.ToString ())) {
                    Style = FontManager.Instance [FontManager.PARTICLE],
                    AlignmentH = Alignment.Center
                }
            });

        }

        void AddResistInformation (Vect2i start, Grouping grouped, Resists resists) {
            grouped.AddWidget (new Label (start, new Vect2i (48, 24), string.Format ("{0}{1:P0}", Colour.Crimson.ToString ("#§"), resists.Heat)) {
                AlignmentH = Alignment.Right
            });
            start += new Vect2i (48, 0);
            grouped.AddWidget (new Label (start, new Vect2i (48, 24), string.Format ("{0}{1:P0}", Colour.Amber.ToString ("#§"), resists.Kinetic)) {
                AlignmentH = Alignment.Right
            });
            start += new Vect2i (48, 0);
            grouped.AddWidget (new Label (start, new Vect2i (48, 24), string.Format ("{0}{1:P0}", Colour.LightGreen.ToString ("#§"), resists.Radiation)) {
                AlignmentH = Alignment.Right
            });
        }

        protected override void Refresh () {
            base.Refresh ();
            ShipInstance ship = _ship != null ? _ship.Value : _grid.Value [_slot];
            if (ship == null) {
                return;
            }

            _scShield.Value = (float)ship.LayerShield / ship.ShipClass.Shield;
            _txShield.Value = ship.LayerShield.ToString ();
            _scArmour.Value = (float)ship.LayerArmour / ship.ShipClass.Armour;
            _txArmour.Value = ship.LayerArmour.ToString ();
            _scHull.Value = (float)ship.LayerHull / ship.ShipClass.Hull;
            _txHull.Value = ship.LayerHull.ToString ();
        }
    }
}

