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
using BLibrary.Saves;
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using BLibrary.Gui.Backgrounds;
using Starliners;

namespace BLibrary.Gui.Interface {

    sealed class GuiSaves : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_saves";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (356, 402);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.MainMenu, true);

        #endregion

        SaveGame _save;
        Label _selected;

        public GuiSaves ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            IsCloseable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            int margin = UIProvider.Margin.X;

            AddHeader (WindowButton.None, Localization.Instance ["menu_loadgame"]);

            AddWidget (CreateSaveSelection (
                CornerTopLeft, new Vect2i (Size.X - 2 * margin, Size.Y - CornerTopLeft.Y - 3 * 44 - margin)));

            _selected = new Label (new Vect2i (CornerTopLeft.X, Size.Y - margin - 3 * 44), new Vect2i (Size.X - 2 * margin, 44), "<Select a file to load>") {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (_selected);

            AddWidget (new Button (new Vect2i (CornerTopLeft.X, Size.Y - margin - 2 * 44), new Vect2i (Size.X - 2 * margin, 40), "loadgame", Localization.Instance ["btn_load"]));
            AddWidget (new Button (new Vect2i (CornerTopLeft.X, Size.Y - margin - 44), new Vect2i (Size.X - 2 * margin, 40), "mainmenu", Localization.Instance ["btn_nav_mainmenu"]));
        }

        Table CreateSaveSelection (Vect2i position, Vect2i size) {

            SaveGame[] saves = SaveUtils.GetSaves (GameAccess.Folders [Constants.PATH_SAVES].Location, Constants.SAVE_SUFFIX);

            Table table = new Table (position, size) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 32
            };

            foreach (SaveGame save in saves) {
                table.AddCellContent (new ListItemSave (new Vect2i (), new Vect2i (size.X, table.RowHeight), "save.slot", save));
                table.NextRow ();
            }

            return table;
        }

        public override bool DoAction (string key, params object[] args) {
            if ("mainmenu".Equals (key)) {
                GuiManager.Instance.CloseGuiAll ();
                GameAccess.Interface.OpenMainMenu ();
                return true;
            } else if ("save.slot.clicked".Equals (key)) {
                _save = ((SaveGame)args [0]);
                _selected.Template = string.Format ("Load '{0}'?", _save.File.Name);
                return true;
            } else if ("loadgame".Equals (key)) {
                if (_save != null) {
                    GameAccess.Interface.CreateGame (_save);
                }
                return true;
            } else
                return base.DoAction (key, args);
        }
    }
}
