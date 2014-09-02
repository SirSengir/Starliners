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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using BLibrary.Util;
using BLauncher.Interface;

namespace BLauncher {

    sealed class LaunchInstance {

        public static LaunchInstance Instance {
            get;
            set;
        }

        #region Properties

        public bool IsLaunchable {
            get;
            private set;
        }

        string Status {
            get {
                return _status;
            }
            set {
                _status = value;
                MainClass.Interface.Invoke (delegate {
                    MainClass.Interface.Current.Status = _status;
                    MainClass.Interface.Current.IsReady = _isComplete;
                });
                MainClass.Logger.Log ("Status: " + _status);
            }
        }

        bool IsComplete {
            get {
                return _isComplete;
            }
            set {
                _isComplete = value;
                MainClass.Interface.Invoke (delegate {
                    MainClass.Interface.Current.Status = _status;
                    MainClass.Interface.Current.IsReady = _isComplete;
                });
            }
        }

        public FileInfo LocalAssetLibraryPath {
            get {
                return _folders [LauncherConstants.PATH_BIN, "manifest.json"];
            }
        }

        public LauncherSettings Settings {
            get;
            private set;
        }

        #endregion

        #region Fields

        PlatformOS _os;

        string _login;
        string _password;
        string _sessionid;
        bool _rememberLogin;
        ChannelInfo _channel;

        string _status;
        bool _isComplete;

        AssetLibrary _local;
        AssetLibrary _remote;

        WebCommunicator _communicator;
        FolderManager _folders;

        #endregion

        #region Constructor

        public LaunchInstance (FolderManager launcherFolders) {
            Settings = new LauncherSettings (launcherFolders);
            _os = PlatformUtils.DeterminePlatform ();
        }

        #endregion

        public void Launch () {
            string args = "-path=" + _folders.InstancePath;
            if (!string.IsNullOrEmpty (_sessionid)) {
                args += " -sessionid=" + _sessionid;
                args += " -login=" + _login;
            }

            string executable = _folders.GetFilePath (LauncherConstants.PATH_BIN, _channel.ExeFile);
            if (_os == PlatformOS.Windows) {
                Process.Start (executable, args);
            } else {
                Process.Start ("./mono", executable + " " + args);
            }
            MainClass.Interface.Quit ();
        }

        /// <summary>
        /// Handle download progress events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadProgress (object sender, DownloadProgressChangedEventArgs e) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            long bytesIn = e.BytesReceived;
            long totalBytes = e.TotalBytesToReceive;
            int percentage = (int)(((float)bytesIn / totalBytes) * 100);
            MainClass.Interface.Invoke (delegate {
                MainClass.Interface.Current.Progress = percentage >= 0 && percentage <= 100 ? percentage : 50;
            });
        }

        /// <summary>
        /// Sets the given channel as a target for launch.
        /// </summary>
        /// <param name="channel">Channel.</param>
        public void SetChannel (ChannelInfo channel) {
            _channel = channel;
            // Reset folders as needed.
            _folders = new FolderManager (_channel.AppRoot, _channel.InstancePath);
            InitDirectories ();
        }

        /// <summary>
        /// Initiates the launch preparations by setting vars and starting to fetch the session ID.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="form"></param>
        public void PrepareLaunch (string login, string password, ChannelInfo channel, bool rememberLogin) {
            _login = string.IsNullOrWhiteSpace (login) ? WebCommunicator.USER_ANON : login;
            _password = password;
            _rememberLogin = rememberLogin;
            SetChannel (channel);

            // Setup the web client.
            _communicator = new WebCommunicator ();
            _communicator.WebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (DownloadProgress);
            _communicator.SessionInited += OnSessionResponse;
            _communicator.ReceivedChannelUpdate += OnChannelUpdate;
            _communicator.ReceivedManifest += OnReceivedManifest;
            _communicator.DownloadOrderComplete += OnDownloadOrderComplete;
            _communicator.DownloadFileStart += OnDownloadFileStart;

            Status = string.Format ("Attempting to fetch session id for '{0}'...", _login);
            BackgroundWorker worker = new BackgroundWorker ();
            worker.DoWork += new DoWorkEventHandler (BeingLaunched);
            worker.RunWorkerAsync ();
        }

        void BeingLaunched (object sender, DoWorkEventArgs args) {
            _communicator.RequestSession (_login, _password);
        }

        void OnSessionResponse (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to create session: {0}", args.Argument1);
                return;
            }

            // Remember the (successful) login
            if (!WebCommunicator.USER_ANON.Equals (_login) && _rememberLogin) {
                SaveLoginInformation ();
            }

            _sessionid = args.Argument1;
            Status = string.Format ("Created session id {0}...", _sessionid);
            sender.UpdateChannels (_login, _sessionid);
        }

        void OnChannelUpdate (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to retrieve channel information: {0}", args.Argument1);
                return;
            }

            File.WriteAllText (_folders [LauncherConstants.PATH_ROOT, "Channels.json"].FullName, args.Argument1);
            //_channels.ParseFromJson (_folders [LauncherConstants.PATH_ROOT, "Channels.json"]);

            Status = string.Format ("Updated channel information.");
            sender.RequestManifset (_login, _sessionid, _channel.Key);
        }

        void OnReceivedManifest (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to retrieve manifest for channel {0}: {1}", _channel.Key, args.Argument1);
                return;
            }

            _remote = new AssetLibrary (args.Argument1);
            if (_remote.IsEmpty) {
                Status = string.Format ("Failed to parse manifest file...");
                return;
            }

            Status = string.Format ("Current version has {0} files...", _remote.FileAssetMap.Count);

            // Compile the list of files needing download.
            _local = new AssetLibrary (LocalAssetLibraryPath);
            IList<FileAsset> outdated = _local.DetermineOutdatedAssets (_remote);

            // Either begin downloading or complete the launch.
            if (outdated.Count > 0) {

                // Offer the option to abort an update if we have an installation, otherwise
                // force the download.
                if (!_local.IsEmpty) {

                    Status = string.Format ("Version {0} is available. Update?", _remote.Version.ToString ());
                    MainClass.Interface.OfferDialogOption (string.Format ("{0} file(s) need to be updated. Do you want to update to version {1} now?", outdated.Count, _remote.Version.ToString ()),
                        delegate {
                            _communicator.DownloadFiles (_login, _sessionid, _channel.Key, _os, _folders [LauncherConstants.PATH_DOWNLOAD].Location, outdated);
                        },
                        delegate {
                            CompletePreparations (true);
                        }
                    );

                } else {
                    Status = string.Format ("Version {0} is available. Downloading...", _remote.Version.ToString ());
                    _communicator.DownloadFiles (_login, _sessionid, _channel.Key, _os, _folders [LauncherConstants.PATH_DOWNLOAD].Location, outdated);
                }
            } else {
                CompletePreparations (true);
            }
        }

        void OnDownloadOrderComplete (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Download failed: {0}", args.Argument1);
                return;
            }

            CompleteUpdate ();
        }

        void OnDownloadFileStart (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (args.Argument0) {
                Status = string.Format ("Downloading '{0}'...", args.Argument1);
            }
        }

        /// <summary>
        /// Save the launcher settings to file.
        /// </summary>
        void SaveLoginInformation () {
            Console.Out.WriteLine ("Saving login information.");
            Settings.Login = _login;
            Settings.Password = _password;
            Settings.Remember = _rememberLogin;
            Settings.LastChannel = _channel.Key;
            Settings.WriteToFile ();
        }

        /// <summary>
        /// Wrap up updating, move the downloads to the actual bin.
        /// </summary>
        void CompleteUpdate () {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            // Backup the old bin and plugin directories
            if (_local.Version != null && !_local.IsEmpty) {
                string backupdir = _folders [LauncherConstants.PATH_BACKUP].Location.FullName + _local.Version.ToString () + "/";
                Status = string.Format ("Making a backup of the existing binaries and plugins to {0}...", backupdir);
                // Binary directory
                FileUtils.MergeDirectories (_folders [LauncherConstants.PATH_BIN].Location,
                    new DirectoryInfo (backupdir + LauncherConstants.PATH_BIN));
                // Plugin directory
                FileUtils.MergeDirectories (_folders [LauncherConstants.PATH_PLUGINS].Location,
                    new DirectoryInfo (backupdir + LauncherConstants.PATH_PLUGINS));
                // Extra directory
                FileUtils.MergeDirectories (_folders [LauncherConstants.PATH_EXTRAS].Location,
                    new DirectoryInfo (backupdir + LauncherConstants.PATH_EXTRAS));
            }

            // Write the new manifest and recreate the local asset library.
            _remote.WriteToManifest (LocalAssetLibraryPath.FullName);
            _local = new AssetLibrary (LocalAssetLibraryPath);
            // Merge the downloaded files with the old bin directory, overwriting files.
            Status = string.Format ("Merging the downloaded files into the bin directory...");
            // Merge downloaded binaries
            FileUtils.MergeDirectories (new DirectoryInfo (_folders.GetFilePath (LauncherConstants.PATH_DOWNLOAD, LauncherConstants.PATH_BIN)),
                _folders [LauncherConstants.PATH_BIN].Location);
            // Merge downloaded plugins
            FileUtils.MergeDirectories (new DirectoryInfo (_folders.GetFilePath (LauncherConstants.PATH_DOWNLOAD, LauncherConstants.PATH_PLUGINS)),
                _folders [LauncherConstants.PATH_PLUGINS].Location);
            // Merge downloaded extras
            FileUtils.MergeDirectories (new DirectoryInfo (_folders.GetFilePath (LauncherConstants.PATH_DOWNLOAD, LauncherConstants.PATH_EXTRAS)),
                _folders [LauncherConstants.PATH_EXTRAS].Location);

            CompletePreparations (false);
        }

        /// <summary>
        /// Finish up the launch process and actually launch the game.
        /// </summary>
        /// <param name="autolaunch"></param>
        void CompletePreparations (bool autolaunch) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            Status = string.Format ("Ready for launch ({0}).", _local.Version.ToString ());
            IsComplete = true;
            if (autolaunch) {
                Launch ();
            }
        }

        void InitDirectories () {
            _folders.DefineFolder (LauncherConstants.PATH_ROOT, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_ROOT, false, true);
            _folders.DefineFolder (LauncherConstants.PATH_BIN, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_BIN, true, true);
            _folders.DefineFolder (LauncherConstants.PATH_BACKUP, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_BACKUP, true, true);
            _folders.DefineFolder (LauncherConstants.PATH_DOWNLOAD, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_DOWNLOAD, true, true);
            _folders.DefineFolder (LauncherConstants.PATH_PLUGINS, Environment.SpecialFolder.Personal, LauncherConstants.PATH_PLUGINS, true, true);
            _folders.DefineFolder (LauncherConstants.PATH_EXTRAS, Environment.SpecialFolder.Personal, LauncherConstants.PATH_EXTRAS, true, true);
        }
            
    }
}
