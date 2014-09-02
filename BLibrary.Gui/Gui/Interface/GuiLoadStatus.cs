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

﻿using BLibrary.Resources;
using BLibrary.Gui.Widgets;
using BLibrary.Util;

namespace BLibrary.Gui.Interface {

    class GuiLoadStatus : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_loadstatus";
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, new Vect2i (448, 76), Positioning.LowerThirdMiddle, false);

        #endregion

        LoadBar _loadbar;

        public GuiLoadStatus ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
        }

        public override void Update () {
            base.Update ();
            _loadbar.FillState = (float)DataProvider.GetValue<int> ("load.current") / DataProvider.GetValue<int> ("load.stages");
        }

        protected override void Regenerate () {
            base.Regenerate ();

            AddHeader (WindowButton.None, Localization.Instance ["game_loading"]);

            Vect2i start = CornerTopLeft;
            AddWidget (_loadbar = new LoadBar (start, new Vect2i (Presets.InnerArea.X, 40), new DataReference<string> (this, "load.status") { Localizable = true }));
        }
    }
}
