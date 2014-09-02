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
using System.Windows.Forms;

namespace BLauncher.Interface.WinForms {
    public sealed class WinInterface : IInterfaceForm {

        public IUpdateWindow Current {
            get {
                return _form;
            }
        }

        public bool IsRunning {
            get {
                return Current != null;
            }
        }

        WinUpdateForm _form;

        public void Init () {
            Application.EnableVisualStyles ();
            _form = new WinUpdateForm ();
        }

        public void Run () {
            _form.ShowDialog ();
            Application.Run ();
        }

        public void Quit () {
            Application.Exit ();
        }

        public void Invoke (EventHandler handler) {
            if (_form.IsHandleCreated) {
                _form.Invoke (handler);
            }
        }

        public void OfferDialogOption (string message, EventHandler affirmative, EventHandler negative) {
            Invoke (delegate {
                DialogResult result = MessageBox.Show (_form, message, string.Empty, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) {
                    affirmative (this, new EventArgs ());
                } else if (result == DialogResult.No) {
                    negative (this, new EventArgs ());
                }
            });

        }
    }
}

