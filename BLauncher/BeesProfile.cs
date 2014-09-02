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
using BLibrary.Json;

namespace BLauncher {
    public partial class BeesProfile : Gtk.Window {

        ProfileUpdater _updater;
        bool _isUpdater;
        string _sessionid;

        public BeesProfile (string login, string password) :
            base (Gtk.WindowType.Toplevel) {
            Build ();

            _updater = new ProfileUpdater ();
            if (!string.IsNullOrWhiteSpace (login)) {
                _tbLogin.Text = login;
                _tbPassword.Text = password;
                _updater.RetrieveRegistration (this, login, password);
            }
        }

        public void SetUpdater () {
            _isUpdater = true;
            _tbLogin.IsEditable = false;
            _tbLogin.Sensitive = false;
            _lblPassword.Text = "New Password:";
            _lblConfirm.Text = "Confirm Password:";
            _btnRegister.Label = "Update";
        }

        public void UpdateRegistrationInfo (string info) {
            _lblRegistrationInfo.Text = info;
        }

        public void UpdateStatus (string status) {
            _lblServerResponse.Text = status;
        }

        public void UpdateSessionId (string sessionid) {
            _sessionid = sessionid;
        }

        public void SetProfileInfo (string json) {
            UpdateRegistrationInfo ("Logged In");
            UpdateStatus ("<...>");
            SetUpdater ();

            JsonObject parsed = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
            _tbLogin.Text = parsed ["login"].GetValue<string> ();
            _tbEmail.Text = parsed ["email"].GetValue<string> ();
            _cbMailings.Active = parsed ["allow_mails"].GetValue<string> ().Equals ("y") ? true : false;
        }

        public void OnSuccessfulRegistration () {
            _updater.RetrieveRegistration (this, _tbLogin.Text, _tbPassword.Text);
        }

        protected void OnRegistrationClicked (object sender, EventArgs e) {
            if (_isUpdater) {
                _updater.UpdateProfile (this, _tbLogin.Text, _sessionid, _tbPassword.Text, _tbConfirm.Text, _tbEmail.Text, _cbMailings.Active);
            } else {
                _updater.RegisterProfile (this, _tbLogin.Text, _tbPassword.Text, _tbConfirm.Text, _tbEmail.Text, _cbMailings.Active);
            }
        }

    }
}

