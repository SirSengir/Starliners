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
using System;
using BLauncher.Interface;
using BLauncher.Interface.Gtk;
using BLauncher.Interface.WinForms;
using System.IO;
using BLibrary.Json;

namespace BLauncher {

    class MainClass {

        public static SimpleLog Logger {
            get;
            private set;
        }

        public static IInterfaceForm Interface {
            get;
            private set;
        }

        static ArgumentHandler _argumentHandler;
        static ArgumentDefinitions _arguments;
        static FolderManager _ownfolders;
        static ChannelInfo _channel;

        public static void Main (string[] args) {

            _argumentHandler = new ArgumentHandler ();
            _argumentHandler.Define (_arguments = new ArgumentDefinitions ());
            _argumentHandler.HandleArgs (args);

            // Set up launcher folders.
            _ownfolders = new FolderManager (LauncherConstants.PATH_APPDATA_LAUNCHER, "Current");
            InitDirectories ();

            Logger = new SimpleLog (_ownfolders [LauncherConstants.PATH_LOG, "Console.log"], true);

            // Get the channel to use.
            JsonArray result = null;
            using (StreamReader reader = new StreamReader (System.Reflection.Assembly.GetEntryAssembly ().GetManifestResourceStream ("BLauncher.Resources.Channels.json"))) {
                result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonArray> ();
                foreach (JsonObject obj in result.GetEnumerable<JsonObject>()) {
                    ChannelInfo channel = new ChannelInfo (obj);
                    if (string.Equals (_arguments.ChannelKey, channel.Key)) {
                        _channel = channel;
                        break;
                    } else if (_channel == null && string.Equals (ChannelDefaults.CHANNEL, channel.Key)) {
                        _channel = channel;
                    }
                }
            }
            Logger.Log ("Launching with channel {0}.", _channel.Key);

            // Determine the interface to use.
            switch (PlatformUtils.DeterminePlatform ()) {
                case PlatformOS.Windows:
                    Interface = new WinInterface ();
                    break;
                default:
                    Interface = new GtkInterface ();
                    break;
            }
            Interface.Init ();

            // Verify that the launcher is up to date
            SelfUpdater selfupdate = new SelfUpdater (_ownfolders);
            if (_arguments.Reload) {
                if (!selfupdate.IsRunnableInstall ()) {
                    Logger.Log ("Closing this instance because another was started.");
                    return;
                }
            } else {
                Logger.Log ("Skipping reload check since it was suppressed.");
            }
            selfupdate.VerificationCompleted += MainClass.RunApp;
            selfupdate.VerifyInstallation ();

            // Run the interface
            Interface.Run ();
        }

        public static void RunApp () {
            LaunchInstance.Instance = new LaunchInstance (_ownfolders);
            LaunchInstance.Instance.PrepareLaunch (WebCommunicator.USER_ANON, string.Empty, _channel, false);
        }

        static void InitDirectories () {
            _ownfolders.DefineFolder (LauncherConstants.PATH_ROOT, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_ROOT, false, true);
            _ownfolders.DefineFolder (LauncherConstants.PATH_BIN, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_BIN, true, true);
            _ownfolders.DefineFolder (LauncherConstants.PATH_LOG, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_LOG, true, true);
            _ownfolders.DefineFolder (LauncherConstants.PATH_BACKUP, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_BACKUP, true, true);
            _ownfolders.DefineFolder (LauncherConstants.PATH_DOWNLOAD, Environment.SpecialFolder.ApplicationData, LauncherConstants.PATH_DOWNLOAD, false, false);
        }

    }
}
