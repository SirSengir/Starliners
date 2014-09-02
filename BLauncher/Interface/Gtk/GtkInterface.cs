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
    public sealed class GtkInterface : IInterfaceForm {

        public bool IsRunning {
            get {
                return _form != null && _form.IsRealized;
            }
        }

        public IUpdateWindow Current {
            get {
                return _form;
            }
        }

        #region Fields

        GtkUpdateForm _form;

        #endregion

        public void Init () {
            Application.Init ();
            _form = new GtkUpdateForm ();
            _form.Show ();
        }

        public void Run () {
            Application.Run ();
        }

        public void Quit () {
            Application.Quit ();
        }

        public void Invoke (EventHandler handler) {
            Application.Invoke (handler);
        }

        public void OfferDialogOption (string message, EventHandler affirmative, EventHandler negative) {
            MessageDialog update = new MessageDialog (_form, 
                                       DialogFlags.DestroyWithParent,
                                       MessageType.Question, 
                                       ButtonsType.YesNo, message);
            Application.Invoke (delegate {
                ResponseType result = (ResponseType)update.Run ();
                update.Destroy ();

                if (result == ResponseType.Yes) {
                    affirmative (this, new EventArgs ());
                } else {
                    negative (this, new EventArgs ());
                }
            });
        }


    }
}

