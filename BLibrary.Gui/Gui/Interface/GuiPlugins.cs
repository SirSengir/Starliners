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
using System.Collections.Generic;
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using BLibrary.Gui.Backgrounds;
using Starliners;

namespace BLibrary.Gui.Interface {

    sealed class GuiPlugins : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_plugins";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (776, 541);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        #region Fields

        ResourceCollection _collection;
        Label _name;
        Label _uid;
        Label _version;
        Label _author;
        TextView _description;
        Switchable _enable;

        #endregion

        public GuiPlugins ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            IsCloseable = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            int margin = UIProvider.Margin.X;

            AddHeader (WindowButton.None, Localization.Instance ["menu_plugins"]);

            Frame pluginselect = CreatePluginSelection (
                                     CornerTopLeft, new Vect2i (Size.X - 2 * margin, Size.Y - CornerTopLeft.Y - 2 * margin - 208 - CornerTopLeft.Y));
            AddWidget (pluginselect);

            Grouping infobox = new Grouping (CornerTopLeft + new Vect2i (0, pluginselect.Size.Y + margin), new Vect2i ((pluginselect.Size.X / 2) - 32, 208));
            AddWidget (infobox);

            Vect2i linespacing = new Vect2i (0, 24);

            _name = new Label (new Vect2i (margin, margin), "--");
            infobox.AddWidget (_name);
            _version = new Label (new Vect2i (margin, margin) + linespacing, string.Empty);
            infobox.AddWidget (_version);
            _author = new Label (new Vect2i (margin, margin) + linespacing * 2, string.Empty);
            infobox.AddWidget (_author);
            _uid = new Label (new Vect2i (margin, margin) + linespacing * 3, string.Empty);
            infobox.AddWidget (_uid);

            _enable = new Switchable (new Vect2i (0, infobox.Size.Y - 40), new Vect2i (infobox.Size.X - margin, 40), "button.enable.toggle", new object[] {
                true,
                false
            }, new string[] {
                "Enabled",
                "Disabled"
            });
            infobox.AddWidget (_enable);

            Grouping descbox = new Grouping (CornerTopLeft + new Vect2i (infobox.Size.X, pluginselect.Size.Y + margin), new Vect2i ((pluginselect.Size.X / 2) + 32, 208)) { Backgrounds = UIProvider.Style.CreateInset () };
            AddWidget (descbox);

            _description = new TextView (new Vect2i (margin, margin), descbox.Size - new Vect2i (margin, margin) * 2, string.Empty);
            descbox.AddWidget (_description);

            ResetPluginInfo ();
            //AddWidget(new Button(new Vect2i(_start.X, WINDOW_SIZE.Y - margin - 40 - _spacing.Y), new Vect2i(Size.X - 2 * margin, 40), "loadgame", Localization.Instance["btn_load"]));
            AddWidget (new Button (new Vect2i (CornerTopLeft.X, Size.Y - margin - 40), new Vect2i (Size.X - 2 * margin, 40), "mainmenu", Localization.Instance ["btn_nav_mainmenu"]));
        }

        Frame CreatePluginSelection (Vect2i position, Vect2i size) {

            IReadOnlyCollection<ResourceCollection> plugins = GameAccess.Resources.Collections;

            Grouping frame = new Grouping (position, size) { Backgrounds = UIProvider.Style.CreateInset () };
            Table table = new Table (new Vect2i (2, 2), size - new Vect2i (4, 4)) {
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION)
            };
            frame.AddWidget (table);
            table.RowHeight = 32;

            foreach (ResourceCollection collection in plugins) {
                table.AddCellContent (new ListItemPlugin (new Vect2i (), new Vect2i (size.X, table.RowHeight), "plugin.slot", collection));
                table.NextRow ();
            }

            return frame;
        }

        void ResetPluginInfo () {
            _name.Template = _collection != null ? _collection.Name : "---";
            _version.Template = string.Format ("Version: {0}", _collection != null ? _collection.Version.ToString () : "---");
            _author.Template = string.Format ("Author: {0}", _collection != null ? _collection.Author : "---");
            _uid.Template = string.Format ("UID: {0}", _collection != null ? _collection.UID : "---");
            _description.ResetText (_collection != null ? _collection.Description : "---");

            if (_collection != null) {
                SetState (ElementState.Disabled, _collection.IsBuiltin);
                _enable.Value = _collection.IsEnabled;
            } else {
                SetState (ElementState.Disabled, true);
            }
        }

        public override bool DoAction (string key, params object[] args) {
            if ("mainmenu".Equals (key)) {
                GuiManager.Instance.CloseGuiAll ();
                GameAccess.Interface.OpenMainMenu ();
                return true;
            } else if ("plugin.slot.clicked".Equals (key)) {
                _collection = ((ResourceCollection)args [0]);
                ResetPluginInfo ();
                return true;
            } else if ("button.enable.toggle".Equals (key)) {
                if (_collection != null) {
                    GameAccess.Resources.SetPluginState (_collection.UID, (bool)_enable.Value);
                }
                ResetPluginInfo ();
                return true;
            } else
                return base.DoAction (key, args);
        }
    }
}
