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
    public partial class BeesInitializer : Window, IUpdateWindow {

        internal BeesInitializer () :
            base (WindowType.Toplevel) {
            Build ();
        }

        public void UpdateStatus (string status, bool complete) {
            _lblStatus.Text = status;
        }

        public void UpdateProgress (int percentage) {
            _progressLaunch.Fraction = (double)percentage / 100;
        }

        protected void OnAbortClick (object sender, EventArgs e) {
            Application.Quit ();
        }

        protected override void OnShown () {
            base.OnShown ();
            KeepAbove = true;
        }
    }
}

