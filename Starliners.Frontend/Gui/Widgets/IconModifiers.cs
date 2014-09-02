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
using BLibrary.Util;
using Starliners.Game.Forces;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui;
using BLibrary.Gui.Widgets;

namespace Starliners.Gui.Widgets {
    sealed class IconModifiers : Widget {
        #region Constants

        static readonly Vect2i DEFAULT_SIZE = new Vect2i (48, 32);
        static readonly Vect2i ICON_SIZE = new Vect2i (16, 16);

        #endregion

        ShipModifiers _modifiers;

        public IconModifiers (Vect2i position, ShipModifiers modifiers)
            : this (position, DEFAULT_SIZE, modifiers) {
        }

        public IconModifiers (Vect2i position, Vect2i size, ShipModifiers modifiers)
            : base (position, size) {
            _modifiers = modifiers;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Vect2i start = new Vect2i (0, 0);
            if (_modifiers.FocusHeat > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolFHeat"));
                start += new Vect2i (16, 0);
            }
            if (_modifiers.FocusKinetic > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolFKinetic"));
                start += new Vect2i (16, 0);
            }
            if (_modifiers.FocusRadiation > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolFRadiation"));
            }

            start = new Vect2i (0, 16);
            if (_modifiers.ResistHeat > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolRHeat"));
                start += new Vect2i (16, 0);
            }
            if (_modifiers.ResistKinetic > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolRKinetic"));
                start += new Vect2i (16, 0);
            }
            if (_modifiers.ResistRadiation > 0) {
                AddWidget (new IconSymbol (start, ICON_SIZE, "symbolFRadiation"));
            }

        }

    }
}

