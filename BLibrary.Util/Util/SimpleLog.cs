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
using System.IO;

namespace BLibrary.Util {
    sealed class SimpleLog {

        bool _console;
        TextWriter _log;

        public SimpleLog (FileInfo file, bool console) {
            _log = TextWriter.Synchronized (new StreamWriter (file.FullName));
            _console = console;
        }

        public void Log (string message, params object[] args) {
            _log.WriteLine (message, args);
            _log.Flush ();
            if (_console) {
                Console.Out.WriteLine (message, args);
            }
        }
    }
}

