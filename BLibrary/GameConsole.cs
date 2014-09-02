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
using System.IO;
using System.Text;
using BLibrary.Util;

namespace BLibrary {

    public enum SLConsoleSender : byte {
        Unknown,
        Interface,
        Simulator
    }

    public sealed class LogLevel {
        public string Key {
            get;
            private set;
        }

        public bool MainLog {
            get;
            set;
        }

        public bool OwnLog {
            get;
            set;
        }

        public bool Console {
            get;
            set;
        }

        public LogLevel (string key) {
            Key = key;
            MainLog = true;
        }
    }
    //Net, AI, Shipyards, Trade, Debug, Info, Message, Warning, Error, Exception }
    sealed class SLConsoleMessage {
        public SLConsoleSender sender;
        public LogLevel level;
        public string message;

        public SLConsoleMessage (SLConsoleSender sender, LogLevel level, string message) {
            this.sender = sender;
            this.level = level;
            this.message = message;
        }
    }

    public class GameConsole {
        #region Fields

        Dictionary<string, LogLevel> _levels = new Dictionary<string, LogLevel> ();
        static Dictionary<string, TextWriter> _logs = new Dictionary<string, TextWriter> ();
        static TextWriter _mainlog;
        SLConsoleSender _sender;

        #endregion

        public static void CreateExceptionString (StringBuilder builder, Exception exception, string indent) {
            if (indent == null)
                indent = string.Empty;
            else if (indent.Length > 0)
                builder.AppendFormat ("{0}Inner ", indent);

            builder.AppendFormat ("Exception Found:\n{0}Type: {1}", indent, exception.GetType ().FullName);
            builder.AppendFormat ("\n{0}Message: {1}", indent, exception.Message);
            builder.AppendFormat ("\n{0}Source: {1}", indent, exception.Source);
            builder.AppendFormat ("\n{0}Stacktrace: {1}", indent, exception.StackTrace);

            if (exception.InnerException != null) {
                builder.Append ("\n");
                CreateExceptionString (builder, exception.InnerException, indent + "  ");
            }
        }

        protected GameConsole (SLConsoleSender sender, ApplicationFolder folder, params LogLevel[] args) {
            _sender = sender;

            _levels ["Serialization"] = new LogLevel ("Serialization");
            _levels ["Debug"] = new LogLevel ("Debug");
            _levels ["Info"] = new LogLevel ("Info");
            _levels ["Warning"] = new LogLevel ("Warning") { Console = true };
            _levels ["Error"] = new LogLevel ("Error");
            _levels ["Exception"] = new LogLevel ("Exception") { OwnLog = true, Console = true };
            _levels ["Network"] = new LogLevel ("Network") { MainLog = false, OwnLog = true, Console = true };
            _levels ["Rendering"] = new LogLevel ("Rendering") { MainLog = false, Console = true };
            _levels ["Audio"] = new LogLevel ("Audio") { MainLog = false, Console = true };
            _levels ["Ai"] = new LogLevel ("Ai") { MainLog = false, Console = true };

            foreach (LogLevel level in args) {
                _levels [level.Key] = level;
            }

            if (_mainlog == null) {
                _mainlog = TextWriter.Synchronized (new StreamWriter (folder.GetFileWithin ("Console.log")));
            }

            foreach (LogLevel level in _levels.Values) {
                if (_logs.ContainsKey (level.Key)) {
                    continue;
                }
                if (!level.OwnLog) {
                    continue;
                }
                _logs [level.Key] = TextWriter.Synchronized (new StreamWriter (folder.GetFileWithin (level.Key + ".log")));
            }
        }

        public void EnableConsoleOutput (params string[] levels) {
            foreach (string level in levels) {
                _levels [level].Console = true;
            }
        }

        public void Serialization (string message, params object[] args) {
            Message (_levels ["Serialization"], message, args);
        }

        public void Debug (string message, params object[] args) {
            Message (_levels ["Debug"], message, args);
        }

        public void Info (string message) {
            Message (_levels ["Info"], message);
        }

        public void Info (string message, params object[] args) {
            Message (_levels ["Info"], message, args);
        }

        public void Warning (string message, params object[] args) {
            Message (_levels ["Warning"], message, args);
        }

        public void Error (string message) {
            Message (_levels ["Error"], message);
        }

        public void Network (string message) {
            Message (_levels ["Network"], message);
        }

        public void Rendering (string message, params object[] args) {
            Message (_levels ["Rendering"], message, args);
        }

        public void Audio (string message, params object[] args) {
            Message (_levels ["Audio"], message, args);
        }

        public void Ai (string message, params object[] args) {
            Message (_levels ["Ai"], message, args);
        }

        public void Log (string level, string message, params object[] args) {
            Message (_levels [level], message, args);
        }

        /// <summary>
        /// Logs a thrown exception.
        /// </summary>
        /// <param name="ex"></param>
        public string Exception (Exception ex) {
            StringBuilder builder = new StringBuilder ();
            CreateExceptionString (builder, ex, string.Empty);
            Console.Out.WriteLine (ex);
            Message (_levels ["Exception"], builder.ToString ());
            return builder.ToString ();
        }

        void Message (LogLevel level, string message, params object[] args) {
            string formattedMessage = string.Format ("[{0}][{1}] {2}", _sender.ToString (), level.Key, string.Format (message, args));
            if (level.OwnLog) {
                _logs [level.Key].WriteLine (formattedMessage);
                _logs [level.Key].Flush ();
            }
            if (level.MainLog) {
                _mainlog.WriteLine (formattedMessage);
                _mainlog.Flush ();
            }
            if (level.Console) {
                Console.WriteLine (formattedMessage);
            }
        }
    }

    public class SLConsoleInterface : GameConsole {
        public SLConsoleInterface (ApplicationFolder logs, params LogLevel[] args)
            : base (SLConsoleSender.Interface, logs, args) {
        }
    }

    public class SLConsoleSimulator : GameConsole {
        public SLConsoleSimulator (ApplicationFolder logs, params LogLevel[] args)
            : base (SLConsoleSender.Simulator, logs, args) {
        }
    }

    public class SLConsoleFallback : GameConsole {
        public SLConsoleFallback (ApplicationFolder logs)
            : base (SLConsoleSender.Unknown, logs) {
        }
    }
}
