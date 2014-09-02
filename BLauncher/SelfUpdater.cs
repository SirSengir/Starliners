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

﻿using BLibrary.Util;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using BLauncher.Interface;

namespace BLauncher {

    delegate void VerificationCompletionHandler ();

    sealed class SelfUpdater {

        #region Events

        public event VerificationCompletionHandler VerificationCompleted;

        #endregion

        #region Properties

        string Status {
            get {
                return _status;
            }
            set {
                _status = value;
                MainClass.Interface.Invoke (delegate {
                    MainClass.Interface.Current.Status = _status;
                });
                MainClass.Logger.Log ("Status: " + _status);
            }
        }

        #endregion

        #region Fields

        WebCommunicator _communicator;
        string _status;

        PlatformOS _os;
        string _sessionid;

        FolderManager _folders;

        AssetLibrary _local;
        AssetLibrary _remote;

        #endregion

        public SelfUpdater (FolderManager folders) {
            _folders = folders;
            _os = PlatformUtils.DeterminePlatform ();
        }

        public bool IsRunnableInstall () {
            DirectoryInfo downloads = _folders [LauncherConstants.PATH_DOWNLOAD].Location;
            DirectoryInfo rundir = new DirectoryInfo (FileUtils.GetAssemblyDirectory ());
            MainClass.Logger.Log ("Download folder is '{0}'.", FileUtils.NormalizePath (downloads.FullName));
            MainClass.Logger.Log ("Assembly dir is '{0}'.", FileUtils.NormalizePath (rundir.FullName));
            MainClass.Logger.Log ("Assembly parent is '{0}'.", FileUtils.NormalizePath (rundir.Parent.FullName));

            // We are the downloaded copy, so we must make ourselves the proper binary and relaunch if possible.
            if (IsTheDownload ()) {
                MainClass.Logger.Log ("Merging downloaded launcher into binary folder.");
                FileUtils.MergeDirectories (new DirectoryInfo (_folders.GetFilePath (LauncherConstants.PATH_DOWNLOAD, LauncherConstants.PATH_BIN)),
                    _folders [LauncherConstants.PATH_BIN].Location);

                // If we are now installed, relaunch into the proper binary.
                if (IsInstalled ()) {
                    LaunchInstalled ();
                    return false;
                } else {
                    return true;
                }
            }

            // Clean up the download directory.
            if (downloads.Exists) {
                MainClass.Logger.Log ("Removing stale download folder.");
                downloads.Delete (true);
            }

            // If we are not the proper installed binary and such a thing exists,
            // we try to relaunch into it.
            if (!IsTheInstall () && IsInstalled ()) {
                MainClass.Logger.Log ("We are not the install, but there is an installed binary, relaunching into it.");
                LaunchInstalled ();
                return false;
            }

            return true;
        }

        public void VerifyInstallation () {

            MainClass.Logger.Log ("Verifying launcher update status.");
            _communicator = new WebCommunicator ();
            _communicator.WebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler (DownloadProgress);
            _communicator.SessionInited += OnSessionResponse;
            _communicator.ReceivedChannelUpdate += OnChannelUpdate;
            _communicator.ReceivedManifest += OnReceivedManifest;
            _communicator.ReceivedNews += OnNewsResponse;
            _communicator.DownloadOrderComplete += OnDownloadOrderComplete;
            _communicator.DownloadFileStart += OnDownloadFileStart;

            Status = string.Format ("Attempting to fetch session id...");
            BackgroundWorker worker = new BackgroundWorker ();
            worker.DoWork += new DoWorkEventHandler (delegate {
                System.Threading.Thread.Sleep (100);
                _communicator.RequestSession (WebCommunicator.USER_ANON, string.Empty);
            });
            worker.RunWorkerAsync ();
        }

        void OnSessionResponse (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                MainClass.Logger.Log ("Interface not running.");
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to create session:\n{0}", args.Argument1);
                return;
            }

            _sessionid = args.Argument1;
            Status = string.Format ("Created session id {0}...", _sessionid);
            sender.UpdateChannels (WebCommunicator.USER_ANON, _sessionid);
        }

        void OnChannelUpdate (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to retrieve channel information:\n{0}", args.Argument1);
                return;
            }

            File.WriteAllText (_folders [LauncherConstants.PATH_ROOT, "Channels.json"].FullName, args.Argument1);

            Status = string.Format ("Updated channel information.");
            sender.RetrieveNews (WebCommunicator.USER_ANON, _sessionid);
        }

        void OnNewsResponse (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to retrieve news:\n{0}", args.Argument1);
                return;
            }

            File.WriteAllText (_folders [LauncherConstants.PATH_ROOT, "News.json"].FullName, args.Argument1);
            //MainClass.News = new NewsList (_folders [LauncherConstants.PATH_ROOT, "News.json"]);

            Status = string.Format ("Updated news.");
            sender.RequestManifset (WebCommunicator.USER_ANON, _sessionid, WebCommunicator.LAUNCHER_CHANNEL);
        }

        void OnReceivedManifest (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Failed to retrieve manifest for channel {0}:\n{1}", WebCommunicator.LAUNCHER_CHANNEL, args.Argument1);
                return;
            }

            _remote = new AssetLibrary (args.Argument1);
            if (_remote.IsEmpty) {
                Status = string.Format ("Failed to parse manifest file...");
                return;
            }

            Status = string.Format ("Current version has {0} files...", _remote.FileAssetMap.Count);

            // Compile the list of files needing download.
            _local = new AssetLibrary (_folders [LauncherConstants.PATH_BIN, "manifest.json"]);
            IList<FileAsset> outdated = _local.DetermineOutdatedAssets (_remote);

            // Either begin downloading or complete the launch.
            if (outdated.Count > 0) {
                Status = string.Format ("Version {0} is available. Downloading...", _remote.Version.ToString ());
                _communicator.DownloadFiles (WebCommunicator.USER_ANON, _sessionid, WebCommunicator.LAUNCHER_CHANNEL, _os, _folders [LauncherConstants.PATH_DOWNLOAD].Location, outdated);
            } else {
                CompletePreparations (false);
            }
        }

        void OnDownloadOrderComplete (WebCommunicator sender, CustomEventArgs<bool, string> args) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            if (!args.Argument0) {
                Status = string.Format ("Download failed:\n{0}", args.Argument1);
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
        /// Wrap up updating, move the downloads to the actual bin.
        /// </summary>
        void CompleteUpdate () {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            // Backup the old bin and plugin directories
            if (_local.Version != null && !_local.IsEmpty) {
                string backupdir = _folders [LauncherConstants.PATH_BACKUP].Location.FullName + _local.Version.ToString () + "/";
                Status = string.Format ("Making a backup of the existing binaries to {0}...", backupdir);
                // Binary directory
                FileUtils.MergeDirectories (_folders [LauncherConstants.PATH_BIN].Location,
                    new DirectoryInfo (backupdir + LauncherConstants.PATH_BIN));
            }

            // Write the new manifest and recreate the local asset library.
            _remote.WriteToManifest (_folders [LauncherConstants.PATH_BIN, "manifest.json"].FullName);
            _local = new AssetLibrary (_folders [LauncherConstants.PATH_BIN, "manifest.json"]);

            CompletePreparations (true);
        }

        /// <summary>
        /// Finish up the launch process and actually launch the game.
        /// </summary>
        /// <param name="autolaunch"></param>
        void CompletePreparations (bool didUpdate) {
            if (!MainClass.Interface.IsRunning) {
                return;
            }

            Status = string.Format ("Ready for launch ({0}).", _local.Version.ToString ());
            if (didUpdate) {
                LaunchDownloaded ();
                MainClass.Interface.Quit ();
            } else {
                VerificationCompleted ();
            }
        }

        bool IsInstalled () {
            FileInfo binary = _folders [LauncherConstants.PATH_BIN, WebCommunicator.LAUNCHER_EXE];
            FileInfo manifest = _folders [LauncherConstants.PATH_BIN, "manifest.json"];
            return binary.Exists && manifest.Exists;
        }

        bool IsTheInstall () {
            FileInfo binary = _folders [LauncherConstants.PATH_BIN, WebCommunicator.LAUNCHER_EXE];
            DirectoryInfo rundir = new DirectoryInfo (FileUtils.GetAssemblyDirectory ());

            return FileUtils.AreMatchingDirectories (binary.Directory, rundir);
        }

        bool IsTheDownload () {
            DirectoryInfo downloads = _folders [LauncherConstants.PATH_DOWNLOAD].Location;
            DirectoryInfo rundir = new DirectoryInfo (FileUtils.GetAssemblyDirectory ());
            return FileUtils.AreMatchingDirectories (rundir.Parent, downloads);
        }

        void LaunchInstalled () {
            Launch (_folders [LauncherConstants.PATH_BIN].Location);
        }

        void LaunchDownloaded () {
            DirectoryInfo downloadexec = new DirectoryInfo (_folders.GetFilePath (LauncherConstants.PATH_DOWNLOAD, LauncherConstants.PATH_BIN));
            Launch (downloadexec);
        }

        void Launch (DirectoryInfo rundir) {
            string executable = Path.Combine (rundir.FullName, WebCommunicator.LAUNCHER_EXE);
            if (_os == PlatformOS.Windows) {
                Process.Start (executable);
            } else {
                Process.Start ("./mono", executable);
            }
        }

    }
}

