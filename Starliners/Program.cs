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
using System.Threading;
using StarLiners.Crash;
using BLibrary.Resources;
using BLibrary.Util;
using BLibrary;
using Starliners.Gui;
using System.Collections.Generic;
using BLibrary.Serialization;

namespace Starliners {

    sealed class Program {
        static Thread _mainThread;

        static void Main (string[] args) {
            _mainThread = Thread.CurrentThread;
            _mainThread.Name = "Interface";

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            GameAccess.CrashReporter = new CrashReporter ();

            SetFromArgs (args);
            InitDirectories ();

            Localization.Instance = new Localization ();
            GameAccess.Game = new GameDefinition ();
            GameAccess.Settings = new SettingsManager ("Settings.json");
            GameAccess.Resources = new ResourceRepository ();

            SetLogin ();

            GameAccess.Simulator = new GameSimulator (new ContainerCreator (), new ActionHandler ());
            GameAccess.Interface = new GameInterface (new InterfaceDefinition (), new GuiCreator ());

            GameAccess.Interface.GameConsole.Info ("======== Starting Starliners ========");
            foreach (string info in GameAccess.Resources.GetVersionInformation()) {
                GameAccess.Interface.GameConsole.Info (info);
            }

            //try {
            GameAccess.Interface.Work ();
            //} catch (Exception ex) {
            //GameAccess.CrashReporter.ReportCrash (ex);
            //}

            GameAccess.CrashReporter.Cleanup ();
        }

        static void SetFromArgs (string[] args) {
            foreach (string arg in args) {
                string[] tokens = arg.Split ('=');
                if (tokens.Length != 2) {
                    Console.Out.WriteLine ("Ignored command line argument '{0}' due to incorrect format.", arg);
                    continue;
                }

                tokens [0] = tokens [0].Replace ("-", "");
                switch (tokens [0]) {
                    case "path":
                        if (!FileUtils.IsValidPathName (tokens [1])) {
                            Console.Out.WriteLine ("Ignored command line argument '{0}' since the given path cannot be created.", arg);
                            continue;
                        }
                        Globals.InstancePath = tokens [1];
                        continue;
                    case "sessionid":
                        Globals.SessionID = tokens [1];
                        continue;
                    case "login":
                        Globals.Login = tokens [1];
                        continue;
                    default:
                        Console.Out.WriteLine ("Ignored command line argument '{0}' since it was not recognized.", arg);
                        continue;
                }
            }
        }

        static void SetLogin () {
            // Logins set from the command line override those in settings.
            if (!string.IsNullOrWhiteSpace (Globals.Login) && !string.Equals (Globals.Login, Globals.LOGIN_ANON)) {
                if (!string.Equals (GameAccess.Settings.Get<string> ("profile", "login"), Globals.Login)) {
                    GameAccess.Settings.Set ("profile", "login", Globals.Login);
                    GameAccess.Settings.Flush ();
                }
                return;
            }

            string login = GameAccess.Settings.Get<string> ("profile", "login");
            Globals.Login = !string.IsNullOrWhiteSpace (login) ? login : Globals.LOGIN_ANON;
        }

        static void OnUnhandledException (object sender, UnhandledExceptionEventArgs e) {
            GameAccess.CrashReporter.ReportCrash (e.ExceptionObject as Exception);
            GameAccess.Interface.Close ();
        }

        public static void InitDirectories () {
            GameAccess.Folders = new FolderManager (LibraryConstants.PATH_APPDATA, Globals.InstancePath);
            GameAccess.Folders.DefineFolder (Constants.PATH_LOG, Environment.SpecialFolder.Personal, Constants.PATH_LOG, true, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_CRASHES, Environment.SpecialFolder.Personal, Constants.PATH_CRASHES, false, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_RESOURCES, Environment.SpecialFolder.Personal, Constants.PATH_RESOURCES, true, false);
            GameAccess.Folders.DefineFolder (Constants.PATH_SAVES, Environment.SpecialFolder.Personal, Constants.PATH_SAVES, true, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_SETTINGS, Environment.SpecialFolder.Personal, Constants.PATH_SETTINGS, true, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_SCREENSHOTS, Environment.SpecialFolder.Personal, Constants.PATH_SCREENSHOTS, false, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_PLUGINS, Environment.SpecialFolder.Personal, Constants.PATH_PLUGINS, true, true);
            GameAccess.Folders.DefineFolder (Constants.PATH_FONTS, Environment.SpecialFolder.ApplicationData, Constants.PATH_FONTS, false, false);
            GameAccess.Folders.DefineFolder (Constants.PATH_TEMP, Environment.SpecialFolder.ApplicationData, Constants.PATH_TEMP, false, true);
        }

    }
}

