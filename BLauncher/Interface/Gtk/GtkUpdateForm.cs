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
using Gtk;

namespace BLauncher.Interface.Gtk {

    public sealed class GtkUpdateForm : Window, IUpdateWindow {

        public bool IsReady {
            get {
                return _isReady;
            }
            set {
                _isReady = value;
                if (_isReady) {
                    _btnUpdater.Label = "Start Game";
                }
            }
        }

        public string Status {
            get {
                return _progressBar.Text;
            }
            set {
                _progressBar.Text = value;
            }
        }

        public int Progress {
            get {
                return (int)(_progressBar.Fraction * 100);
            }
            set {
                _progressBar.Fraction = (double)value / 100;
            }
        }

        #region Serialization

        bool _isReady;
        ProgressBar _progressBar;
        Button _btnUpdater;

        #endregion

        public GtkUpdateForm () :
            base (WindowType.Toplevel) {
            Build ();

            _btnUpdater.Clicked += new EventHandler (OnBtnUpdaterClick);
        }

        void OnBtnUpdaterClick (object sender, EventArgs e) {
            if (IsReady) {
                LaunchInstance.Instance.Launch ();
            } else {
                MainClass.Interface.Quit ();
            }
        }

        protected override void OnShown () {
            base.OnShown ();
            KeepAbove = true;
        }

        void Build () {

            Name = "BLauncher.Interface.Gtk.GtkUpdateForm";
            Title = "Launching...";
            Icon = global::Gdk.Pixbuf.LoadFromResource ("BLauncher.Resources.Icon.ico");
            WindowPosition = ((global::Gtk.WindowPosition)(1));
            DefaultWidth = 350;
            DefaultHeight = 66;
            Resizable = false;

            VBox vbox = new VBox (false, 4);

            _progressBar = new ProgressBar ();
            _progressBar.Name = "_progressLaunch";
            _progressBar.WidthRequest = DefaultWidth - 12;
            _progressBar.HeightRequest = 24;
            vbox.PackStart (_progressBar, false, false, 0);

            _btnUpdater = new Button ();
            _btnUpdater.CanFocus = true;
            _btnUpdater.Name = "_btnUpdater";
            _btnUpdater.UseUnderline = true;
            _btnUpdater.Label = "Cancel";
            _btnUpdater.WidthRequest = DefaultWidth - 48;
            _btnUpdater.HeightRequest = 32;
            vbox.PackEnd (_btnUpdater, false, false, 0);

            Add (vbox);

            if ((Child != null)) {
                Child.ShowAll ();
            }
            Show ();
        }
    }

}

