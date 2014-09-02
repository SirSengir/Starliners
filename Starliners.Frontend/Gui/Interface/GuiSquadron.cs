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

namespace Starliners.Gui.Interface {
    sealed class GuiSquadron : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (512, 256);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_quadron", WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        public GuiSquadron (int containerId)
            : base (WINDOW_SETTING, containerId) {
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (DEFAULT_BUTTONS, new DataReference<string> (this, "gui.header"));
        }
    }
}

