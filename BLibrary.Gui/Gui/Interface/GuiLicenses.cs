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
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using Starliners;


namespace BLibrary.Gui.Interface {

    class GuiLicenses : GuiText {
        public GuiLicenses ()
            : base (GameAccess.Resources.SearchResource ("Text.Licenses.txt"), Localization.Instance ["menu_licenses"], Alignment.Left) {

        }

        protected override void Regenerate () {
            base.Regenerate ();
            int margin = UIProvider.Margin.X;
            Vect2i buttons = new Vect2i (margin, Size.Y - 40 - margin);

            AddWidget (new Button (buttons, new Vect2i (396, 40), "mainmenu", Localization.Instance ["btn_nav_mainmenu"]));
        }

        public override bool DoAction (string key, params object[] args) {
            if ("mainmenu".Equals (key)) {
                GuiManager.Instance.CloseGuiAll ();
                GameAccess.Interface.OpenMainMenu ();
                return true;
            }
            return false;
        }
    }
}
