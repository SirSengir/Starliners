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
using System.Linq;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using Starliners;
using BLibrary.Gui;
using BLibrary.Gui.Interface;
using BLibrary;

namespace Starliners.Gui.Interface {

    public class GuiMenu : GuiLocal {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 160);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("menu_main", WINDOW_SIZE, Positioning.LowerMiddle, false) {
            Style = string.Empty
        };

        const string BUTTON_PROFILE = "button.profile";
        const string BUTTON_CONTINUE = "button.continue";
        const string BUTTON_CREATE = "button.create";
        const string BUTTON_LOAD = "button.load";
        const string BUTTON_JOIN = "button.join";
        const string BUTTON_SETTINGS = "button.settings";
        const string BUTTON_PLUGINS = "button.plugins";
        const string BUTTON_ABOUT = "button.about";
        const string BUTTON_EXIT = "button.exit";

        #endregion

        public GuiMenu ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            IsCloseable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            Vect2i btnsize = new Vect2i ((Presets.InnerArea.X - 3 * UIProvider.Margin.X) / 4, 40);
            Vect2i rowspacing = new Vect2i (0, 56);
            Vect2i colspacing = new Vect2i (btnsize.X + UIProvider.Margin.X, 0);
            int count = 0;

            Grouping row0 = new Grouping (CornerTopLeft, new Vect2i (Presets.InnerArea.X, btnsize.Y)) {
                AlignmentH = Alignment.Center
            };
            AddWidget (row0);
            row0.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_PROFILE,
                new TextComposition (new TextComponent ("btn_game_profile") { IsLocalizable = true }, new DataLink<string> (delegate {
                    return Globals.Login;
                })))));

            Grouping row1 = new Grouping (CornerTopLeft + rowspacing, new Vect2i (Presets.InnerArea.X, btnsize.Y)) {
                AlignmentH = Alignment.Center
            };
            AddWidget (row1);

            count = 0;
            Button btnContinue = StyleButton (new Button (colspacing * count++, btnsize, BUTTON_CONTINUE, Localization.Instance ["btn_game_continue"]));
            btnContinue.SetState (ElementState.Disabled, SaveUtils.GetSaves (GameAccess.Folders [Constants.PATH_SAVES].Location, Constants.SAVE_SUFFIX).FirstOrDefault () == null);
            row1.AddWidget (btnContinue);
            row1.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_CREATE, Localization.Instance ["btn_game_create"])));
            row1.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_LOAD, Localization.Instance ["btn_game_load"])));
            row1.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_JOIN, Localization.Instance ["btn_game_join"])));

            Grouping row2 = new Grouping (CornerTopLeft + rowspacing * 2, row1.Size) {
                AlignmentH = Alignment.Center
            };
            AddWidget (row2);

            count = 0;
            row2.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_SETTINGS, Localization.Instance ["btn_game_settings"])));
            row2.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_PLUGINS, Localization.Instance ["btn_game_plugins"])));
            row2.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_ABOUT, Localization.Instance ["menu_about"])));
            row2.AddWidget (StyleButton (new Button (colspacing * count++, btnsize, BUTTON_EXIT, Localization.Instance ["btn_game_exit"])));
        }

        Button StyleButton (Button button) {
            button.Tinting = NoTinting.INSTANCE;
            button.Backgrounds = UIProvider.Styles ["mainmenu"].ButtonStyle.CreateBackgrounds ();
            return button;
        }

        public override bool DoAction (string key, params object[] args) {

            switch (key) {
                case BUTTON_PROFILE:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiProfile ());
                    break;
                case BUTTON_CREATE:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiSetup ());
                    break;
                case BUTTON_CONTINUE:
                    SaveGame recent = SaveUtils.GetSaves (GameAccess.Folders [Constants.PATH_SAVES].Location, Constants.SAVE_SUFFIX).FirstOrDefault ();
                    if (recent != null) {
                        GameAccess.Interface.CreateGame (recent);
                    }
                    break;
                case BUTTON_LOAD:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiSaves ());
                    break;
                case BUTTON_JOIN:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiJoin ());
                    break;
                case BUTTON_SETTINGS:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiSettings ());
                    break;
                case BUTTON_PLUGINS:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiPlugins ());
                    break;
                case BUTTON_ABOUT:
                    GuiManager.Instance.CloseGuiAll ();
                    GuiManager.Instance.OpenGui (new GuiAbout ());
                    break;
                case BUTTON_EXIT:
                    GameAccess.Interface.Close ();
                    break;
                default:
                    return false;
            }

            return true;

        }
    }
}
