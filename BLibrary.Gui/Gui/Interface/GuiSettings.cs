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
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Gui.Interface {

    sealed class GuiSettings : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_settings";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (356, 402);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.MainMenu, true);

        #endregion

        Label _modified;

        public GuiSettings ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            IsCloseable = false;

        }

        protected override void Regenerate () {
            base.Regenerate ();

            Vect2i start = UIProvider.Margin;
            Vect2i column1 = new Vect2i (128, 0);
            Vect2i spacing = new Vect2i (0, 44);
            int count = 0;

            AddHeader (WindowButton.None, Localization.Instance ["menu_settings"]);

            Grouping frame = new Grouping (CornerTopLeft, new Vect2i (Size.X - 2 * UIProvider.Margin.X, Presets.InnerArea.Y - 80)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (frame);

            frame.AddWidget (new Label (spacing * count, new Vect2i (column1.X, 40), Localization.Instance ["setting_videomode"]) {
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            });
            frame.AddWidget (new Switchable (spacing * count++ + column1, new Vect2i (Size.X - column1.X - 2 * UIProvider.Margin.X, 40), "setting.videomode",
                new object[] { "window", "fullscreen" },
                new string[] { Localization.Instance ["setting_window"], Localization.Instance ["setting_fullscreen"] }) { Value = GameAccess.Settings.Get<string> ("video", "mode") }
            );

            DisplayResolution[] validModes = DisplayDevice.Default.AvailableResolutions.ToArray ();
            List<string> modesDisplay = new List<string> ();
            DisplayResolution previous = null;
            for (int i = 0; i < validModes.Length; i++) {
                if (previous != null && previous.Width == validModes [i].Width && previous.Height == validModes [i].Height)
                    continue;
                if (validModes [i].Width < 1024)
                    continue;
                if (validModes [i].Height < 768)
                    continue;
                modesDisplay.Add (string.Format ("{0}x{1}", validModes [i].Width, validModes [i].Height));
                previous = validModes [i];
            }

            frame.AddWidget (new Label (spacing * count, new Vect2i (column1.X, 40), Localization.Instance ["setting_resolution"]) {
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            });
            frame.AddWidget (new Switchable (spacing * count++ + column1, new Vect2i (Size.X - column1.X - 2 * UIProvider.Margin.X, 40), "setting.resolution",
                modesDisplay.ToArray (),
                modesDisplay.ToArray ()) { Value = GameAccess.Settings.Get<string> ("video", "resolution") }
            );

            frame.AddWidget (new Label (spacing * count, new Vect2i (column1.X, 40), Localization.Instance ["setting_shadows"]) {
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            });
            frame.AddWidget (new Switchable (spacing * count++ + column1, new Vect2i (Size.X - column1.X - 2 * UIProvider.Margin.X, 40), "setting.shadows",
                new object[] { true, false },
                new string[] { "Enabled", "Disabled" }) { Value = GameAccess.Settings.Get<bool> ("video", "shadows") }
            );

            frame.AddWidget (new Label (spacing * count, new Vect2i (column1.X, 40), Localization.Instance ["setting_sound"]) {
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            });
            frame.AddWidget (new Switchable (spacing * count++ + column1, new Vect2i (Size.X - column1.X - 2 * UIProvider.Margin.X, 40), "setting.sound",
                new object[] { true, false },
                new string[] { "Enabled", "Disabled" }) { Value = GameAccess.Settings.Get<bool> ("sound", "effects") }
            );

            frame.AddWidget (new Label (spacing * count, new Vect2i (column1.X, 40), Localization.Instance ["setting_music"]) {
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            });
            frame.AddWidget (new Switchable (spacing * count++ + column1, new Vect2i (Size.X - column1.X - 2 * UIProvider.Margin.X, 40), "setting.music",
                new object[] { true, false },
                new string[] { "Enabled", "Disabled" }) { Value = GameAccess.Settings.Get<bool> ("sound", "music") }
            );

            _modified = new Label (start + spacing * 8, new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), Localization.Instance ["apply_changes"]) {
                IsDisplayed = false,
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            };
            AddWidget (_modified);
            AddWidget (new Button (new Vect2i (start.X, Size.Y - UIProvider.Margin.X - 40), new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), "mainmenu", Localization.Instance ["btn_nav_mainmenu"]));
        }

        void ApplySettings () {
            GameAccess.Interface.ChangeResolution (Vect2i.Parse (GameAccess.Settings.Get<string> ("video", "resolution")), "fullscreen".Equals (GameAccess.Settings.Get<string> ("video", "mode")));
        }

        public override bool DoAction (string key, params object[] args) {

            if ("mainmenu".Equals (key)) {
                ApplySettings ();
                GuiManager.Instance.CloseGuiAll ();
                GameAccess.Interface.OpenMainMenu ();
            } else if ("setting.videomode".Equals (key)) {
                _modified.IsDisplayed = true;
                GameAccess.Settings.Set ("video", "mode", args [0]);
                GameAccess.Settings.Flush ();
            } else if ("setting.resolution".Equals (key)) {
                _modified.IsDisplayed = true;
                GameAccess.Settings.Set ("video", "resolution", args [0]);
                GameAccess.Settings.Flush ();
            } else if ("setting.shadows".Equals (key)) {
                //_modified.IsDisplayed = true;
                GameAccess.Settings.Set ("video", "shadows", args [0]);
                GameAccess.Settings.Flush ();
            } else if ("setting.sound".Equals (key)) {
                _modified.IsDisplayed = true;
                GameAccess.Settings.Set ("sound", "effects", args [0]);
                GameAccess.Settings.Flush ();
            } else if ("setting.music".Equals (key)) {
                _modified.IsDisplayed = true;
                GameAccess.Settings.Set ("sound", "music", args [0]);
                GameAccess.Settings.Flush ();
            } else
                return false;

            return true;

        }
    }
}
