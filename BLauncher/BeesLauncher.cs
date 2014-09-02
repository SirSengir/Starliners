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
using BLibrary.Util;

namespace BLauncher {

    public partial class BeesLauncher : Gtk.Window {

        public static BeesLauncher Instance {
            get;
            set;
        }

        Version _version;
        BeesProfile _profile;
        ChangelogUpdater _changelogUpdater;

        public BeesLauncher () : base (Gtk.WindowType.Toplevel) {
            Build ();

            _changelogUpdater = new ChangelogUpdater ();
            MainClass.Changelog.LogsChanged += delegate {
                Application.Invoke (delegate {
                    OnChangelogsChanged ();
                });
            };

            _llLauncherInfo.Text = PlatformUtils.GetEXEVersion ().ToString ();
            _lbNETVersion.Text = PlatformUtils.GetCLRInformation (true);

            _tbLogin.Text = LaunchInstance.Instance.Settings.Login;
            _tbLogin.TextInserted += new TextInsertedHandler (_tbLogin_TextInserted);
            _tbLogin.TextDeleted += new TextDeletedHandler (_tbLogin_TextDeleted);

            _tbPassword.Text = LaunchInstance.Instance.Settings.Password;
            _cbLoginRemember.Active = LaunchInstance.Instance.Settings.Remember;

            _btnLogin.Clicked += new EventHandler (_btnLogin_Click);
            _btnOffline.Clicked += new EventHandler (_btnOffline_Click);

            ResetChannelList ();
            if (!string.IsNullOrWhiteSpace (LaunchInstance.Instance.Settings.LastChannel)) {
                SetChannel (LaunchInstance.Instance.Settings.LastChannel);
            }

            ResetInstallInformation ();
            ResetLoginButton ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected override void OnShown () {
            base.OnShown ();
            ResetInstallInformation ();
            ResetLoginButton ();
            ResetChannelList ();
            PopulateNewsList ();
        }

        void OnChangelogsChanged () {
            _tvChangelog.Buffer.Clear ();
            TextTag tag = new TextTag (null);
            _tvChangelog.Buffer.TagTable.Add (tag);
            tag.Weight = Pango.Weight.Bold;

            TextIter iter = _tvChangelog.Buffer.GetIterAtLine (0);
            foreach (Changelogs.VersionInformation ver in MainClass.Changelog.Versions) {
                _tvChangelog.Buffer.InsertWithTags (ref iter, string.Format ("{0}\n", ver.Version.ToString ()), tag);
                iter.ForwardLine ();

                foreach (Changelogs.Change change in ver.Changes) {
                    _tvChangelog.Buffer.Insert (ref iter, string.Format ("{0}: {1}\n", change.Level.ToString (), change.Description));
                    iter.ForwardLine ();
                }
                _tvChangelog.Buffer.Insert (ref iter, "\n");
                iter.ForwardLine ();
            }
        }

        void ResetInstallInformation () {
            AssetLibrary local = new AssetLibrary (LaunchInstance.Instance.LocalAssetLibraryPath);
            if (local.Version == null) {
                _version = null;
                _llVersionInfo.Text = "<Not Installed>";
                _btnOffline.Sensitive = false;
            } else {
                _version = local.Version;
                _llVersionInfo.Text = _version.ToString ();
                _btnOffline.Sensitive = true;
            }
        }

        void ResetLoginButton () {
            if (_tbLogin.Text == null || _tbLogin.Text.Length < 3) {
                _btnLogin.Sensitive = false;
            } else {
                _btnLogin.Sensitive = true;
            }
        }

        void ResetChannelList () {
            _cbChannelList.Clear (); 

            ListStore listStore = new ListStore (typeof(ChannelInfo), typeof(string));
            _cbChannelList.Model = listStore; 
            CellRendererText text = new CellRendererText (); 
            _cbChannelList.PackStart (text, false); 
            _cbChannelList.AddAttribute (text, "text", 1); 

            foreach (var entry in LaunchInstance.Instance.ChannelDefinitions.Channels) { 
                listStore.AppendValues (entry, entry.Name); 
            } 

            TreeIter iter; 
            if (listStore.GetIterFirst (out iter)) { 
                _cbChannelList.SetActiveIter (iter); 
            } 
        }

        ChannelInfo GetSelectedChannel () {
            TreeIter iter;
            _cbChannelList.GetActiveIter (out iter);
            return (ChannelInfo)_cbChannelList.Model.GetValue (iter, 0);
        }

        void SetChannel (string key) {
            ChannelInfo channel = null;

            Gtk.TreeIter iter;
            _cbChannelList.Model.GetIterFirst (out iter);

            do {
                GLib.Value thisRow = new GLib.Value ();
                _cbChannelList.Model.GetValue (iter, 0, ref thisRow);
                if (string.Equals ((thisRow.Val as ChannelInfo).Key, key)) {
                    _cbChannelList.SetActiveIter (iter);
                    channel = thisRow.Val as ChannelInfo;
                    break;
                }
            } while (_cbChannelList.Model.IterNext (ref iter));

            SetChannelInfo (channel);
        }

        void SetChannelInfo (ChannelInfo channel) {
            if (channel == null) {
                return;
            }

            _changelogUpdater.FetchChangelog (channel.Key);
            _tvInfo.Buffer.Clear ();

            TextTag tag = new TextTag (null);
            _tvInfo.Buffer.TagTable.Add (tag);
            tag.Weight = Pango.Weight.Bold;

            TextIter iter = _tvInfo.Buffer.GetIterAtLine (0);
            _tvInfo.Buffer.InsertWithTags (ref iter, string.Format ("{0}\n", channel.Name), tag);

            iter = _tvInfo.Buffer.GetIterAtLine (2);
            _tvInfo.Buffer.Insert (ref iter, string.Format ("{0}\n", channel.Description));
        }

        void PopulateNewsList () {
            _tvNews.Buffer.Clear ();
            TextTag tag = new TextTag (null);
            _tvNews.Buffer.TagTable.Add (tag);
            tag.Weight = Pango.Weight.Bold;

            TextIter iter = _tvNews.Buffer.GetIterAtLine (0);
            foreach (NewsList.NewsItem item in MainClass.News.Items) {
                _tvNews.Buffer.InsertWithTags (ref iter, string.Format ("{0} - {1}\n", item.Date.ToShortDateString (), item.Title), tag);
                iter.ForwardLine ();
                _tvNews.Buffer.Insert (ref iter, string.Format ("{0}\n\n", item.Content));
                iter.ForwardLine ();
            }
        }

        protected void OnChannelChanged (object sender, EventArgs e) {
            ChannelInfo channel = GetSelectedChannel ();
            LaunchInstance.Instance.SetChannel (channel);
            ResetInstallInformation ();
            SetChannelInfo (channel);
        }

        protected void OnRegistrationClicked (object sender, EventArgs e) {
            _profile = new BeesProfile (_tbLogin.Text, _tbPassword.Text);
            _profile.Show ();
        }

        void _btnLogin_Click (object sender, EventArgs args) {
            BeesUpdater form = new BeesUpdater ();
            form.Show ();

            LaunchInstance.Instance.PrepareLaunch (_tbLogin.Text, _tbPassword.Text, GetSelectedChannel (), form, _cbLoginRemember.Active);
            Hide ();
        }

        void _btnOffline_Click (object sender, EventArgs args) {
            LaunchInstance.Instance.Launch ();
            Application.Quit ();
        }

        void _tbLogin_TextDeleted (object sender, TextDeletedArgs e) {
            ResetLoginButton ();
        }

        void _tbLogin_TextInserted (object sender, TextInsertedArgs e) {
            ResetLoginButton ();
        }

    }

}
