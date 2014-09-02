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

    public sealed class GuiExit : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_exit";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (384, 160);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true);

        const string BUTTON_QUIT = "button.quit";
        const string BUTTON_RETIRE = "button.retire";
        const string BUTTON_SAVE = "button.save";

        #endregion

        public GuiExit ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (WindowButton.Close, Localization.Instance ["gui_exit"]);

            Grouping grouped = new Grouping (CornerTopLeft, Presets.InnerArea) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);

            int buttoncount = 0;
            if (GameAccess.Interface.IsInGame) {
                grouped.AddWidget (new Button (new Vect2i (0, buttoncount++ * 44), new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), BUTTON_SAVE, Localization.Instance ["btn_game_save"]));
                grouped.AddWidget (new Button (new Vect2i (0, buttoncount++ * 44), new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), BUTTON_RETIRE, Localization.Instance ["btn_game_retire_save"]));
            }
            grouped.AddWidget (new Button (new Vect2i (0, buttoncount++ * 44), new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), BUTTON_QUIT, Localization.Instance ["btn_game_quit_save"]));
        }

        public override bool DoAction (string key, params object[] args) {
            switch (key) {
                case BUTTON_QUIT:
                    GameAccess.Interface.Close ();
                    return true;
                case BUTTON_RETIRE:
                    GuiManager.Instance.CloseGuiAll ();
                    GameAccess.Interface.Retire ();
                    return true;
                case BUTTON_SAVE:
                    GameAccess.Interface.Controller.RequestSave ();
                    GuiManager.Instance.CloseGui (Presets.Key);
                    return true;
                default:
                    return base.DoAction (key, args);
            }
        }
    }
}
