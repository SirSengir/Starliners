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

namespace BLibrary.Util {
    sealed class ArgumentHandler {

        List<IArgumentDefinition> _definitions = new List<IArgumentDefinition> ();

        public void Define (IArgumentDefinition definition) {
            _definitions.Add (definition);
        }

        public string GetParams (string key, string[] args) {
            foreach (string arg in args) {
                string[] tokens = arg.Split ('=');
                if (tokens.Length != 2) {
                    Console.Out.WriteLine ("Ignored command line argument '{0}' due to incorrect format.", arg);
                    continue;
                }

                tokens [0] = tokens [0].Replace ("-", "");
                if (string.Equals (key, tokens [0])) {
                    return tokens [1];
                }
            }
            return string.Empty;
        }

        public void HandleArgs (string[] args) {
            foreach (string arg in args) {
                string[] tokens = arg.Split ('=');
                if (tokens.Length != 2) {
                    Console.Out.WriteLine ("Ignored command line argument '{0}' due to incorrect format.", arg);
                    continue;
                }

                tokens [0] = tokens [0].Replace ("-", "");
                foreach (IArgumentDefinition definition in _definitions) {
                    definition.HandleArgument (tokens [0], tokens [1]);
                }
            }
        }
    }
}

