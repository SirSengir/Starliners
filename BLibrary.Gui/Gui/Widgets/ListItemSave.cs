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
using OpenTK.Input;
using System.Collections.Generic;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary.Gui.Tooltips;
using Starliners;
using Starliners.Game;

namespace BLibrary.Gui.Widgets {

    public sealed class ListItemSave : Widget {
        SaveGame _save;
        PluginState _state = PluginState.Available;

        public ListItemSave (Vect2i position, Vect2i size, string key, SaveGame save)
            : base (position, size, key) {

            _save = save;

            if (_save.Header.Version.CompareTo (PlatformUtils.GetEXEVersion ()) < 0) {
                _state = PluginState.Outdated;
            }

            List<string> info = new List<string> ();
            info.Add (Localization.Instance ["tt_save_name", _save.Header.Name]);
            info.Add ((_state != PluginState.Available ? "§#" + Colour.Crimson.ToString ("X") + "§" : "") + Localization.Instance ["tt_save_version", _save.Header.Version]);
            info.Add (Localization.Instance ["tt_save_date", _save.File.LastWriteTime.ToShortDateString ()]);
            if (save.Header.Plugins.Length > 0)
                info.Add ("§#" + Colour.CornflowerBlue.ToString ("X") + "§" + Localization.Instance ["tt_save_plugins"]);
            foreach (PluginInfo plugin in save.Header.Plugins) {

                string statecolour = Colour.Crimson.ToString ("X");
                switch (GameAccess.Resources.GetPluginState (plugin.UID, plugin.Version)) {
                    case PluginState.Available:
                        statecolour = Colour.Chartreuse.ToString ("X");
                        break;
                    case PluginState.Outdated:
                        _state = PluginState.Outdated;
                        statecolour = Colour.Orange.ToString ("X");
                        break;
                    default:
                        _state = PluginState.Missing;
                        break;
                }
                info.Add (string.Format ("§#{0}§{1} (v{2})", statecolour, GameAccess.Resources.GetPluginName (plugin.UID), plugin.Version.ToString ()));
            }

            FixedTooltip = new TooltipSimple (_save.File.Name, info.ToArray ());

        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddWidget (new Label (new Vect2i (), new Vect2i (Size.X - 128 - 64, Size.Y), _save.Header.Name) { AlignmentV = Alignment.Center });
            AddWidget (new Label (new Vect2i (Size.X - 112 - 80, 0), new Vect2i (112, Size.Y), (new GameClock (_save.Header.Ticks)).FileFormat) { AlignmentV = Alignment.Center });
            AddWidget (new Label (new Vect2i (Size.X - 80, 0), new Vect2i (80, Size.Y), _save.Header.Version.ToString ()) { AlignmentV = Alignment.Center });
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            if (!IntersectsWith (coordinates)) {
                return base.HandleMouseClick (coordinates, button);
            }
            Window.DoAction ("save.slot.clicked", _save);
            return true;
        }
    }
}
