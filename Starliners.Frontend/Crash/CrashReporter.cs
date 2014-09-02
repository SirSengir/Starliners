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
using BLibrary;
using Starliners;

namespace StarLiners.Crash {

    public sealed class CrashReporter : ICrashReporter {

        CrashReport _report;

        public void ReportCrash (Exception ex) {
            _report = new CrashReport (ex);
            _report.Save ();
        }

        public void Cleanup () {
            if (_report != null) {
                HandleCrash (_report);
            }
            _report = null;
        }

        static void HandleCrash (CrashReport report) {
            if (GameAccess.Interface != null) {
                GameAccess.Interface.GameConsole.Exception (report.Exception);
            } else {
                Console.Out.WriteLine (report.ToString ());
            }

            try {
                //new ExceptionBox (report).ShowDialog ();
            } catch (Exception ex) {
                Console.Out.WriteLine (ex.ToString ());
            }
        }

    }
}

