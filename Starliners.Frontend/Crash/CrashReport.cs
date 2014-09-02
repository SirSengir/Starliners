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
using System.Text;
using System.IO;
using Starliners;

namespace StarLiners.Crash {

    sealed class CrashReport {

        public long Timestamp {
            get;
            private set;
        }

        public Exception Exception {
            get;
            private set;
        }

        public string Logfile {
            get {
                return GameAccess.Folders.GetFilePath (Constants.PATH_CRASHES, string.Format ("crash_{0}.txt", Timestamp));
            }
        }

        public CrashReport (Exception exception) {
            Exception = exception;
            Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Save the crash report as a text file.
        /// </summary>
        public void Save () {
            File.WriteAllText (Logfile, ToString ());
        }

        public override string ToString () {
            StringBuilder builder = new StringBuilder ();

            builder.AppendLine ("=========================================");
            builder.AppendLine ("=\t\tCRASH REPORT\t\t=");
            builder.AppendLine ("=========================================");
            builder.AppendLine ("=");
            foreach (string info in GameAccess.Resources.GetVersionInformation()) {
                builder.AppendLine (info);
            }
            builder.AppendFormat ("= TimeStamp: {0}", new DateTime (Timestamp));
            builder.AppendLine ();
            builder.AppendLine ();
            builder.AppendLine ("=========================================");
            builder.AppendLine ("=\t\tSTACKTRACE\t\t=");
            builder.AppendLine ("=========================================");

            CreateExceptionString (builder, Exception, string.Empty);
            return builder.ToString ();
        }

        void CreateExceptionString (StringBuilder builder, Exception exception, string indent) {
            if (indent == null) {
                indent = string.Empty;
            } else if (indent.Length > 0) {
                builder.AppendFormat ("{0}Inner ", indent);
            }

            builder.AppendFormat ("Exception Found:\n{0}Type: {1}", indent, exception.GetType ().FullName);
            builder.AppendFormat ("\n{0}Message: {1}", indent, exception.Message);
            builder.AppendFormat ("\n{0}Source: {1}", indent, exception.Source);
            builder.AppendFormat ("\n{0}Stacktrace: {1}", indent, exception.StackTrace);

            if (exception.InnerException != null) {
                builder.Append ("\n");
                CreateExceptionString (builder, exception.InnerException, indent + "  ");
            }
        }
    }
}
